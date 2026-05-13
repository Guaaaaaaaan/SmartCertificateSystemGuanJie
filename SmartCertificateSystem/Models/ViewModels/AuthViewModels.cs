using System.ComponentModel.DataAnnotations;

namespace SmartCertificateSystem.Models.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(40)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(240)]
    public string Address { get; set; } = string.Empty;

    [StringLength(140)]
    public string CompanyName { get; set; } = string.Empty;

    [EmailAddress, StringLength(160)]
    public string CompanyEmail { get; set; } = string.Empty;
}

public record AuthResult(bool Success, string Message, int? UserId = null, string? FullName = null, string? Role = null);
