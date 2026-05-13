using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartCertificateSystem.Models;

namespace SmartCertificateSystem.Models.ViewModels;

public class StudentFormViewModel
{
    public int UserId { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password), MinLength(6)]
    public string? Password { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);

    [StringLength(40)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(240)]
    public string Address { get; set; } = string.Empty;

    [Range(0, 4)]
    public double GPA { get; set; }
}

public class StudentsIndexViewModel
{
    public string? Search { get; set; }
    public string Sort { get; set; } = "name";
    public List<Student> Students { get; set; } = [];
}

public class CourseManagementViewModel
{
    public List<Course> Courses { get; set; } = [];
    public List<Student> Students { get; set; } = [];
    public CourseFormViewModel CourseForm { get; set; } = new();
    public ModuleFormViewModel ModuleForm { get; set; } = new();
    public EnrollmentFormViewModel EnrollmentForm { get; set; } = new();
}

public class CourseFormViewModel
{
    public int CourseId { get; set; }

    [Required, StringLength(160)]
    public string CourseName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class ModuleFormViewModel
{
    public int ModuleId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required, StringLength(120)]
    public string ModuleName { get; set; } = string.Empty;

    [Range(1, 10)]
    public int CreditValue { get; set; } = 1;
}

public class EnrollmentFormViewModel
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int StudentId { get; set; }
}

public class FileUploadViewModel
{
    public List<SelectListItem> Students { get; set; } = [];
    public List<SelectListItem> Certificates { get; set; } = [];
    public List<SelectListItem> Transcripts { get; set; } = [];

    [Required]
    public int StudentId { get; set; }

    public int? CertificateId { get; set; }
    public int? TranscriptId { get; set; }

    [Required]
    public string Category { get; set; } = "Certificate";

    public IFormFile? File { get; set; }
}

public class CertificateCreateViewModel
{
    public List<SelectListItem> Students { get; set; } = [];

    [Required]
    public int StudentId { get; set; }

    [Required, StringLength(40), Display(Name = "Certificate ID")]
    public string CertificateId { get; set; } = string.Empty;

    [Required, StringLength(160), Display(Name = "Award Title")]
    public string AwardTitle { get; set; } = string.Empty;

    [Required, DataType(DataType.Date), Display(Name = "Completion Date")]
    public DateTime CompletionDate { get; set; } = DateTime.Today;
}

public class CertificatesIndexViewModel
{
    public List<Certificate> Certificates { get; set; } = [];
    public CertificateCreateViewModel CreateForm { get; set; } = new();
}

public class AdminDashboardViewModel
{
    public int StudentCount { get; set; }
    public int CertificateCount { get; set; }
    public int CourseCount { get; set; }
    public int RawSqlStudentCount { get; set; }
}
