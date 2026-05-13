using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Database;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var gpaCalculator = scope.ServiceProvider.GetRequiredService<GpaCalculator>();

        Directory.CreateDirectory(Path.Combine(environment.ContentRootPath, "App_Data"));
        Directory.CreateDirectory(Path.Combine(environment.ContentRootPath, "FileStorage", "Certificates"));
        Directory.CreateDirectory(Path.Combine(environment.ContentRootPath, "FileStorage", "Transcripts"));
        Directory.CreateDirectory(Path.Combine(environment.ContentRootPath, "FileStorage", "StudentDocuments"));

        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var admin = new Admin
        {
            FullName = "System Admin",
            Email = "admin@example.com",
            PasswordHash = PasswordHasher.Hash("Admin123!"),
            Role = UserRoles.Admin,
            StaffId = 1001
        };

        var student = new Student
        {
            FullName = "Alan Tan",
            Email = "student@example.com",
            PasswordHash = PasswordHasher.Hash("Student123!"),
            Role = UserRoles.Student,
            StudentId = 2026001,
            DateOfBirth = new DateTime(2000, 5, 15),
            Phone = "+65 9123 4567",
            Address = "SkillsFuture Campus",
            GPA = 3.67
        };

        var employer = new Employer
        {
            FullName = "Grace Lee",
            Email = "employer@example.com",
            PasswordHash = PasswordHasher.Hash("Employer123!"),
            Role = UserRoles.Employer,
            CompanyName = "Future Talent Pte Ltd",
            CompanyEmail = "hr@futuretalent.example"
        };

        db.Users.AddRange(admin, student, employer);
        await db.SaveChangesAsync();

        var course = new Course
        {
            CourseName = "Diploma in Software Development",
            Description = "C# programming, object-oriented design, database systems, and web development."
        };
        course.Modules.Add(new Module { ModuleName = "Object-Oriented Design", CreditValue = 4 });
        course.Modules.Add(new Module { ModuleName = "C# Programming", CreditValue = 4 });
        course.Modules.Add(new Module { ModuleName = "Database Development", CreditValue = 3 });
        course.Enrollments.Add(new Enrollment { StudentId = student.UserId });

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var grades = new List<Grade>
        {
            CreateGrade(gpaCalculator, "Object-Oriented Design", 88, 4),
            CreateGrade(gpaCalculator, "C# Programming", 82, 4),
            CreateGrade(gpaCalculator, "Database Development", 76, 3)
        };
        var transcriptPath = CreateSeedFile(environment, "Transcripts", "alan_tan_transcript.pdf", "Seed transcript file for Alan Tan.");
        var transcript = new Transcript
        {
            StudentId = student.UserId,
            GeneratedDate = DateTime.UtcNow.AddDays(-7),
            Grades = grades,
            GPA = gpaCalculator.CalculateGpa(grades),
            FilePath = transcriptPath
        };
        student.GPA = transcript.GPA;

        db.Transcripts.Add(transcript);
        await db.SaveChangesAsync();

        var certificatePath = CreateSeedFile(environment, "Certificates", "SC-2026-0001.pdf", "Seed certificate PDF placeholder.");
        db.Certificates.AddRange(
            new Certificate
            {
                StudentId = student.UserId,
                TranscriptId = transcript.TranscriptId,
                CertificateId = "SC-2026-0001",
                AwardTitle = "Diploma in Software Development",
                IssueDate = DateTime.Today.AddDays(-30),
                CompletionDate = new DateTime(2026, 4, 20),
                Status = CertificateStatuses.Valid,
                FilePath = certificatePath
            },
            new Certificate
            {
                StudentId = student.UserId,
                CertificateId = "SC-2026-0002",
                AwardTitle = "Certificate in Legacy Systems",
                IssueDate = DateTime.Today.AddYears(-1),
                CompletionDate = DateTime.Today.AddYears(-1).AddDays(-10),
                Status = CertificateStatuses.Revoked
            });

        await db.SaveChangesAsync();
    }

    private static Grade CreateGrade(GpaCalculator gpaCalculator, string moduleName, double score, int credits) => new()
    {
        ModuleName = moduleName,
        Score = score,
        LetterGrade = gpaCalculator.GetLetterGrade(score),
        CreditValue = credits
    };

    private static string CreateSeedFile(IWebHostEnvironment environment, string folder, string fileName, string contents)
    {
        var folderPath = Path.Combine(environment.ContentRootPath, "FileStorage", folder);
        Directory.CreateDirectory(folderPath);
        var absolutePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(absolutePath))
        {
            File.WriteAllText(absolutePath, contents);
        }

        return Path.Combine("FileStorage", folder, fileName).Replace('\\', '/');
    }
}
