using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Services;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Tests;

public class ServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly string _root;
    private readonly InputValidator _validator = new();
    private readonly GpaCalculator _gpaCalculator = new();

    public ServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _root = Path.Combine(Path.GetTempPath(), "smart-certificate-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        SeedTestData();
    }

    [Fact]
    public async Task Register_rejects_duplicate_email()
    {
        var auth = new AuthService(_db, _validator);

        var result = await auth.RegisterAsync(new RegisterViewModel
        {
            FullName = "Duplicate Student",
            Email = "student@example.com",
            Password = "Student123!",
            Role = UserRoles.Student,
            DateOfBirth = new DateTime(2001, 1, 1)
        });

        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message);
    }

    [Fact]
    public async Task Login_rejects_wrong_password()
    {
        var auth = new AuthService(_db, _validator);

        var result = await auth.LoginAsync("student@example.com", "WrongPassword");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Admin_can_add_update_and_delete_student()
    {
        var service = new StudentService(_db);
        var student = await service.AddStudentAsync(new StudentFormViewModel
        {
            FullName = "Beth Lim",
            Email = "beth@example.com",
            Password = "Student123!",
            DateOfBirth = new DateTime(2002, 2, 2),
            GPA = 3.1
        });

        await service.UpdateStudentAsync(new StudentFormViewModel
        {
            UserId = student.UserId,
            FullName = "Bethany Lim",
            Email = "bethany@example.com",
            DateOfBirth = new DateTime(2002, 2, 2),
            GPA = 3.4
        });

        var updated = await service.GetStudentAsync(student.UserId);
        Assert.Equal("Bethany Lim", updated!.FullName);

        await service.DeleteStudentAsync(student.UserId);
        Assert.Null(await service.GetStudentAsync(student.UserId));
    }

    [Fact]
    public async Task Certificate_verification_succeeds_with_matching_details()
    {
        var service = BuildCertificateService();

        var result = await service.VerifyCertificate("SC-2026-0001", "Alan Tan", new DateTime(2000, 5, 15));

        Assert.True(result.IsValid);
        Assert.Equal("Diploma in Software Development", result.AwardTitle);
        Assert.NotNull(result.TranscriptPath);
        Assert.NotNull(result.TranscriptId);
    }

    [Fact]
    public async Task Certificate_verification_fails_when_certificate_is_missing()
    {
        var service = BuildCertificateService();

        var result = await service.VerifyCertificate("missing", "Alan Tan", new DateTime(2000, 5, 15));

        Assert.False(result.IsValid);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task Certificate_verification_fails_when_identity_does_not_match()
    {
        var service = BuildCertificateService();

        var result = await service.VerifyCertificate("SC-2026-0001", "Wrong Name", new DateTime(2000, 5, 15));

        Assert.False(result.IsValid);
        Assert.Contains("mismatch", result.Message);
    }

    [Fact]
    public async Task Certificate_verification_reports_missing_transcript_file()
    {
        var certificate = await _db.Certificates.Include(c => c.Transcript).FirstAsync(c => c.CertificateId == "SC-2026-0001");
        certificate.Transcript!.FilePath = "FileStorage/Transcripts/missing.pdf";
        await _db.SaveChangesAsync();
        var service = BuildCertificateService();

        var result = await service.VerifyCertificate("SC-2026-0001", "Alan Tan", new DateTime(2000, 5, 15));

        Assert.True(result.IsValid);
        Assert.Null(result.TranscriptPath);
        Assert.Contains("unavailable", result.Message);
    }

    [Fact]
    public void Gpa_calculator_uses_weighted_grade_points()
    {
        var grades = new[]
        {
            new Grade { Score = 88, CreditValue = 4 },
            new Grade { Score = 76, CreditValue = 2 }
        };

        var gpa = _gpaCalculator.CalculateGpa(grades);

        Assert.Equal(3.83, gpa);
    }

    [Fact]
    public async Task Search_and_sort_students_work()
    {
        var service = new SearchSortService(_db);

        var search = await service.SearchStudentByName("Alan");
        var byName = await service.SortStudentsAlphabetically();
        var byGpa = await service.SortStudentsByGpa();
        var cert = await service.SearchCertificateById("SC-2026-0001");

        Assert.Single(search);
        Assert.Equal(byName.OrderBy(s => s.FullName).Select(s => s.UserId), byName.Select(s => s.UserId));
        Assert.Equal(byGpa.OrderByDescending(s => s.GPA).Select(s => s.UserId), byGpa.Select(s => s.UserId));
        Assert.NotNull(cert);
    }

    [Fact]
    public async Task Course_service_can_update_and_delete_courses_and_modules()
    {
        var service = new CourseService(_db);
        var course = await service.AddCourse(new CourseFormViewModel
        {
            CourseName = "Cybersecurity Basics",
            Description = "Security fundamentals"
        });
        await service.AddModule(new ModuleFormViewModel
        {
            CourseId = course.CourseId,
            ModuleName = "Secure Coding",
            CreditValue = 2
        });
        var module = await _db.Modules.FirstAsync(m => m.CourseId == course.CourseId);

        await service.UpdateCourse(new CourseFormViewModel
        {
            CourseId = course.CourseId,
            CourseName = "Cybersecurity Essentials",
            Description = "Updated"
        });
        await service.UpdateModule(new ModuleFormViewModel
        {
            ModuleId = module.ModuleId,
            CourseId = course.CourseId,
            ModuleName = "Threat Modelling",
            CreditValue = 3
        });

        var updated = await _db.Courses.Include(c => c.Modules).FirstAsync(c => c.CourseId == course.CourseId);
        Assert.Equal("Cybersecurity Essentials", updated.CourseName);
        Assert.Equal("Threat Modelling", updated.Modules.Single().ModuleName);

        await service.DeleteModule(module.ModuleId);
        Assert.False(await _db.Modules.AnyAsync(m => m.ModuleId == module.ModuleId));

        await service.DeleteCourse(course.CourseId);
        Assert.False(await _db.Courses.AnyAsync(c => c.CourseId == course.CourseId));
    }

    [Fact]
    public void File_service_rejects_downloads_outside_filestorage()
    {
        var fileService = new FileService(_root, _validator);
        var outsideFile = Path.Combine(_root, "outside.txt");
        File.WriteAllText(outsideFile, "outside");

        Assert.Throws<UnauthorizedAccessException>(() => fileService.ReadStoredFile(outsideFile));
    }

    [Fact]
    public async Task Valid_certificate_transcript_lookup_rejects_revoked_certificate()
    {
        var student = await _db.Students.FirstAsync();
        var transcript = new Transcript
        {
            StudentId = student.UserId,
            GPA = 2.0,
            FilePath = "FileStorage/Transcripts/revoked.pdf"
        };
        _db.Transcripts.Add(transcript);
        await _db.SaveChangesAsync();

        _db.Certificates.Add(new Certificate
        {
            CertificateId = "SC-2026-REVOKED",
            StudentId = student.UserId,
            TranscriptId = transcript.TranscriptId,
            AwardTitle = "Revoked Award",
            CompletionDate = new DateTime(2026, 1, 1),
            IssueDate = new DateTime(2026, 1, 2),
            Status = CertificateStatuses.Revoked
        });
        await _db.SaveChangesAsync();
        var service = BuildCertificateService();

        var result = await service.GetTranscriptForValidCertificateAsync(transcript.TranscriptId);

        Assert.Null(result);
    }

    private CertificateService BuildCertificateService()
    {
        var fileService = new FileService(_root, _validator);
        return new CertificateService(_db, fileService, _validator, new RawSqlHelper(_db));
    }

    private void SeedTestData()
    {
        var transcriptRelativePath = Path.Combine("FileStorage", "Transcripts", "test_transcript.pdf").Replace('\\', '/');
        var transcriptAbsolutePath = Path.Combine(_root, transcriptRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(transcriptAbsolutePath)!);
        File.WriteAllText(transcriptAbsolutePath, "test transcript");

        var student = new Student
        {
            FullName = "Alan Tan",
            Email = "student@example.com",
            PasswordHash = PasswordHasher.Hash("Student123!"),
            Role = UserRoles.Student,
            StudentId = 2026001,
            DateOfBirth = new DateTime(2000, 5, 15),
            GPA = 3.67
        };

        _db.Students.Add(student);
        _db.SaveChanges();

        var transcript = new Transcript
        {
            StudentId = student.UserId,
            GPA = 3.67,
            FilePath = transcriptRelativePath,
            Grades =
            [
                new Grade { ModuleName = "Object-Oriented Design", Score = 88, LetterGrade = "A", CreditValue = 4 },
                new Grade { ModuleName = "C# Programming", Score = 82, LetterGrade = "B+", CreditValue = 4 }
            ]
        };

        _db.Transcripts.Add(transcript);
        _db.SaveChanges();

        _db.Certificates.Add(new Certificate
        {
            CertificateId = "SC-2026-0001",
            StudentId = student.UserId,
            TranscriptId = transcript.TranscriptId,
            AwardTitle = "Diploma in Software Development",
            CompletionDate = new DateTime(2026, 4, 20),
            IssueDate = new DateTime(2026, 4, 27),
            Status = CertificateStatuses.Valid
        });
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
