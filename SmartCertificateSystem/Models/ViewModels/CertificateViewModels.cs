using System.ComponentModel.DataAnnotations;
using SmartCertificateSystem.Models;

namespace SmartCertificateSystem.Models.ViewModels;

public class CertificateVerificationViewModel
{
    [Required, Display(Name = "Certificate ID")]
    public string CertificateId { get; set; } = string.Empty;

    [Required, Display(Name = "Student Name")]
    public string StudentName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date), Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);

    public VerificationResult? Result { get; set; }
}

public class StudentDashboardViewModel
{
    public Student Student { get; set; } = new();
    public List<Certificate> Certificates { get; set; } = [];
    public List<Transcript> Transcripts { get; set; } = [];
}
