using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCertificateSystem.Models;

public interface IVerifiable
{
    bool Verify();
    string GetVerificationDetails();
}

public interface ISearchable
{
    List<Student> SearchByName(string name);
    object? SearchById(string id);
}

public static class UserRoles
{
    public const string Student = "Student";
    public const string Admin = "Admin";
    public const string Employer = "Employer";
}

public static class CertificateStatuses
{
    public const string Valid = "Valid";
    public const string Revoked = "Revoked";
    public const string Expired = "Expired";
}

public abstract class User
{
    public int UserId { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Role { get; set; } = string.Empty;

    public abstract bool Login(string email, string password);
    public void Logout() { }
    public abstract string GetRole();
}

public class Student : User
{
    public int StudentId { get; set; }
    public DateTime DateOfBirth { get; set; }

    [StringLength(40)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(240)]
    public string Address { get; set; } = string.Empty;

    [Column(TypeName = "decimal(4,2)")]
    public double GPA { get; set; }

    public List<Certificate> Certificates { get; set; } = [];
    public List<Transcript> Transcripts { get; set; } = [];
    public List<Enrollment> Enrollments { get; set; } = [];

    public Transcript? ViewTranscript() => Transcripts.OrderByDescending(t => t.GeneratedDate).FirstOrDefault();
    public bool VerifyOwnCertificate(string certificateId) => Certificates.Any(c => c.CertificateId == certificateId && c.Verify());
    public override bool Login(string email, string password) => Email.Equals(email, StringComparison.OrdinalIgnoreCase);
    public override string GetRole() => UserRoles.Student;
}

public class Admin : User
{
    public int StaffId { get; set; }

    public void AddStudent(Student student) { }
    public void UpdateStudent(Student student) { }
    public void DeleteStudent(int studentId) { }
    public Transcript GenerateTranscript(int studentId) => new() { StudentId = studentId, GeneratedDate = DateTime.UtcNow };
    public void AddCourse(Course course) { }
    public override bool Login(string email, string password) => Email.Equals(email, StringComparison.OrdinalIgnoreCase);
    public override string GetRole() => UserRoles.Admin;
}

public class Employer : User
{
    [StringLength(140)]
    public string CompanyName { get; set; } = string.Empty;

    [EmailAddress, StringLength(160)]
    public string CompanyEmail { get; set; } = string.Empty;

    public VerificationResult VerifyCertificate(string certificateId, string studentName, DateTime dob) =>
        new(false, "Use CertificateService to verify certificates.", null, null, null);

    public override bool Login(string email, string password) => Email.Equals(email, StringComparison.OrdinalIgnoreCase);
    public override string GetRole() => UserRoles.Employer;
}

public class Certificate : IVerifiable
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string CertificateId { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string AwardTitle { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }
    public DateTime CompletionDate { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = CertificateStatuses.Valid;

    [StringLength(500)]
    public string? FilePath { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int? TranscriptId { get; set; }
    public Transcript? Transcript { get; set; }

    public bool Verify() => IsValid();
    public string GetVerificationDetails() => $"{AwardTitle} completed on {CompletionDate:yyyy-MM-dd} ({Status})";
    public bool IsValid() => Status.Equals(CertificateStatuses.Valid, StringComparison.OrdinalIgnoreCase);
    public string GetCertificateDetails() => $"{CertificateId} - {GetVerificationDetails()}";
}

public class Transcript
{
    public int TranscriptId { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public double GPA { get; set; }

    [StringLength(500)]
    public string? FilePath { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public List<Grade> Grades { get; set; } = [];

    public double CalculateGPA() => Grades.Count == 0 ? 0 : Math.Round(Grades.Average(g => g.CalculateGradePoint()), 2);
    public string DisplayGrades() => string.Join(", ", Grades.Select(g => $"{g.ModuleName}: {g.LetterGrade}"));
    public string GenerateTranscriptFile() => FilePath ?? string.Empty;
}

public class Grade
{
    public int GradeId { get; set; }

    [Required, StringLength(120)]
    public string ModuleName { get; set; } = string.Empty;

    public double Score { get; set; }

    [Required, StringLength(4)]
    public string LetterGrade { get; set; } = string.Empty;

    public int CreditValue { get; set; } = 1;

    public int TranscriptId { get; set; }
    public Transcript? Transcript { get; set; }

    public double CalculateGradePoint() => Score switch
    {
        >= 85 => 4.0,
        >= 75 => 3.5,
        >= 65 => 3.0,
        >= 55 => 2.5,
        >= 50 => 2.0,
        _ => 0
    };
}

public class Course
{
    public int CourseId { get; set; }

    [Required, StringLength(160)]
    public string CourseName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public List<Module> Modules { get; set; } = [];
    public List<Enrollment> Enrollments { get; set; } = [];

    public void AddModule(Module module) => Modules.Add(module);
    public List<Student> ViewEnrolledStudents() => Enrollments.Where(e => e.Student != null).Select(e => e.Student!).ToList();
}

public class Module
{
    public int ModuleId { get; set; }

    [Required, StringLength(120)]
    public string ModuleName { get; set; } = string.Empty;

    public int CreditValue { get; set; } = 1;
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public string GetModuleInfo() => $"{ModuleName} ({CreditValue} credits)";
}

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
}

public record VerificationResult(
    bool IsValid,
    string Message,
    string? AwardTitle,
    DateTime? CompletionDate,
    string? TranscriptPath)
{
    public string DisplayResult() => IsValid ? $"Valid certificate: {AwardTitle}" : Message;
}
