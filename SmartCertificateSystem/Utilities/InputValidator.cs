using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;

namespace SmartCertificateSystem.Utilities;

public class InputValidator
{
    private static readonly string[] AllowedFileExtensions = [".pdf", ".txt", ".doc", ".docx", ".png", ".jpg", ".jpeg"];

    public bool ValidateCertificateInput(string certificateId, string studentName, DateTime dateOfBirth, out string message)
    {
        if (string.IsNullOrWhiteSpace(certificateId))
        {
            message = "Certificate ID is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(studentName))
        {
            message = "Student name is required.";
            return false;
        }

        if (dateOfBirth > DateTime.Today)
        {
            message = "Date of birth cannot be in the future.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public bool ValidateRegistration(RegisterViewModel model, out string message)
    {
        if (model.Role == UserRoles.Student && model.DateOfBirth is null)
        {
            message = "Students must provide a date of birth.";
            return false;
        }

        if (model.Role == UserRoles.Employer && string.IsNullOrWhiteSpace(model.CompanyName))
        {
            message = "Employers must provide a company name.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public bool IsAllowedFile(string fileName) =>
        AllowedFileExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
}
