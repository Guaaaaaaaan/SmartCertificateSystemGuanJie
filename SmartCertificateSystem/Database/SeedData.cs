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

        var mei = new Student
        {
            FullName = "Mei Chen",
            Email = "mei.chen@example.com",
            PasswordHash = PasswordHasher.Hash("Student123!"),
            Role = UserRoles.Student,
            StudentId = 2026002,
            DateOfBirth = new DateTime(2001, 8, 12),
            Phone = "+65 9234 5678",
            Address = "Tampines Learning Centre"
        };

        var ravi = new Student
        {
            FullName = "Ravi Kumar",
            Email = "ravi.kumar@example.com",
            PasswordHash = PasswordHasher.Hash("Student123!"),
            Role = UserRoles.Student,
            StudentId = 2026003,
            DateOfBirth = new DateTime(1999, 11, 3),
            Phone = "+65 9345 6789",
            Address = "Jurong Campus"
        };

        var sophia = new Student
        {
            FullName = "Sophia Lim",
            Email = "sophia.lim@example.com",
            PasswordHash = PasswordHasher.Hash("Student123!"),
            Role = UserRoles.Student,
            StudentId = 2026004,
            DateOfBirth = new DateTime(2002, 2, 24),
            Phone = "+65 9456 7890",
            Address = "Woodlands Training Hub"
        };

        var aisyah = new Student
        {
            FullName = "Nur Aisyah",
            Email = "nur.aisyah@example.com",
            PasswordHash = PasswordHasher.Hash("Student123!"),
            Role = UserRoles.Student,
            StudentId = 2026005,
            DateOfBirth = new DateTime(2000, 9, 9),
            Phone = "+65 9567 8901",
            Address = "Central Campus"
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

        db.Users.AddRange(admin, student, mei, ravi, sophia, aisyah, employer);
        await db.SaveChangesAsync();

        var course = new Course
        {
            CourseName = "Diploma in Software Development",
            Description = "C# programming, object-oriented design, database systems, and web development."
        };
        course.Modules.Add(new Module { ModuleName = "Object-Oriented Design", CreditValue = 4 });
        course.Modules.Add(new Module { ModuleName = "C# Programming", CreditValue = 4 });
        course.Modules.Add(new Module { ModuleName = "Database Development", CreditValue = 3 });

        var webCourse = new Course
        {
            CourseName = "Advanced Web Applications",
            Description = "ASP.NET Core MVC, REST APIs, deployment, and user interface implementation."
        };
        webCourse.Modules.Add(new Module { ModuleName = "ASP.NET Core MVC", CreditValue = 4 });
        webCourse.Modules.Add(new Module { ModuleName = "REST API Design", CreditValue = 3 });
        webCourse.Modules.Add(new Module { ModuleName = "Cloud Deployment", CreditValue = 3 });
        webCourse.Modules.Add(new Module { ModuleName = "UI/UX Fundamentals", CreditValue = 2 });

        var dataCourse = new Course
        {
            CourseName = "Data Analytics Foundations",
            Description = "Data preparation, statistics, database querying, and dashboard reporting."
        };
        dataCourse.Modules.Add(new Module { ModuleName = "Python for Data Analysis", CreditValue = 4 });
        dataCourse.Modules.Add(new Module { ModuleName = "Statistics for Analytics", CreditValue = 3 });
        dataCourse.Modules.Add(new Module { ModuleName = "Data Visualisation", CreditValue = 3 });

        var cyberCourse = new Course
        {
            CourseName = "Cybersecurity Essentials",
            Description = "Network security, secure coding practices, and incident response basics."
        };
        cyberCourse.Modules.Add(new Module { ModuleName = "Network Security", CreditValue = 3 });
        cyberCourse.Modules.Add(new Module { ModuleName = "Secure Coding", CreditValue = 3 });
        cyberCourse.Modules.Add(new Module { ModuleName = "Incident Response", CreditValue = 2 });

        db.Courses.AddRange(course, webCourse, dataCourse, cyberCourse);
        await db.SaveChangesAsync();

        db.Enrollments.AddRange(
            new Enrollment { StudentId = student.UserId, CourseId = course.CourseId },
            new Enrollment { StudentId = mei.UserId, CourseId = course.CourseId },
            new Enrollment { StudentId = ravi.UserId, CourseId = webCourse.CourseId },
            new Enrollment { StudentId = sophia.UserId, CourseId = dataCourse.CourseId },
            new Enrollment { StudentId = aisyah.UserId, CourseId = cyberCourse.CourseId },
            new Enrollment { StudentId = mei.UserId, CourseId = webCourse.CourseId });
        await db.SaveChangesAsync();

        var transcriptPath = CreateSeedFile(environment, "Transcripts", "alan_tan_transcript.pdf", "Seed transcript file for Alan Tan.");
        var transcript = CreateTranscript(gpaCalculator, student, transcriptPath, 7,
            ("Object-Oriented Design", 88, 4),
            ("C# Programming", 82, 4),
            ("Database Development", 76, 3));
        var meiTranscript = CreateTranscript(gpaCalculator, mei, null, 5,
            ("Object-Oriented Design", 91, 4),
            ("C# Programming", 87, 4),
            ("Database Development", 80, 3));
        var raviTranscript = CreateTranscript(gpaCalculator, ravi, null, 4,
            ("ASP.NET Core MVC", 84, 4),
            ("REST API Design", 79, 3),
            ("Cloud Deployment", 72, 3));
        var sophiaTranscript = CreateTranscript(gpaCalculator, sophia, null, 3,
            ("Python for Data Analysis", 89, 4),
            ("Statistics for Analytics", 74, 3),
            ("Data Visualisation", 86, 3));
        var aisyahTranscript = CreateTranscript(gpaCalculator, aisyah, null, 2,
            ("Network Security", 77, 3),
            ("Secure Coding", 83, 3),
            ("Incident Response", 68, 2));

        student.GPA = transcript.GPA;
        mei.GPA = meiTranscript.GPA;
        ravi.GPA = raviTranscript.GPA;
        sophia.GPA = sophiaTranscript.GPA;
        aisyah.GPA = aisyahTranscript.GPA;

        db.Transcripts.AddRange(transcript, meiTranscript, raviTranscript, sophiaTranscript, aisyahTranscript);
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
            },
            new Certificate
            {
                StudentId = mei.UserId,
                TranscriptId = meiTranscript.TranscriptId,
                CertificateId = "SC-2026-0003",
                AwardTitle = "Diploma in Software Development",
                IssueDate = DateTime.Today.AddDays(-18),
                CompletionDate = new DateTime(2026, 4, 22),
                Status = CertificateStatuses.Valid
            },
            new Certificate
            {
                StudentId = ravi.UserId,
                TranscriptId = raviTranscript.TranscriptId,
                CertificateId = "SC-2026-0004",
                AwardTitle = "Advanced Web Applications",
                IssueDate = DateTime.Today.AddDays(-10),
                CompletionDate = new DateTime(2026, 5, 1),
                Status = CertificateStatuses.Valid
            },
            new Certificate
            {
                StudentId = sophia.UserId,
                TranscriptId = sophiaTranscript.TranscriptId,
                CertificateId = "SC-2026-0005",
                AwardTitle = "Data Analytics Foundations",
                IssueDate = DateTime.Today.AddDays(-40),
                CompletionDate = new DateTime(2026, 3, 28),
                Status = CertificateStatuses.Valid
            },
            new Certificate
            {
                StudentId = aisyah.UserId,
                TranscriptId = aisyahTranscript.TranscriptId,
                CertificateId = "SC-2026-0006",
                AwardTitle = "Cybersecurity Essentials",
                IssueDate = DateTime.Today.AddMonths(-8),
                CompletionDate = DateTime.Today.AddMonths(-8).AddDays(-7),
                Status = CertificateStatuses.Expired
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

    private static Transcript CreateTranscript(
        GpaCalculator gpaCalculator,
        Student student,
        string? filePath,
        int daysAgo,
        params (string ModuleName, double Score, int Credits)[] grades)
    {
        var gradeRows = grades
            .Select(g => CreateGrade(gpaCalculator, g.ModuleName, g.Score, g.Credits))
            .ToList();

        return new Transcript
        {
            StudentId = student.UserId,
            GeneratedDate = DateTime.UtcNow.AddDays(-daysAgo),
            Grades = gradeRows,
            GPA = gpaCalculator.CalculateGpa(gradeRows),
            FilePath = filePath
        };
    }

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
