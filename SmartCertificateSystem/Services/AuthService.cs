using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Services;

public class AuthService(AppDbContext db, InputValidator validator)
{
    private readonly AppDbContext _db = db;
    private readonly InputValidator _validator = validator;

    public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
    {
        if (!_validator.ValidateRegistration(model, out var validationMessage))
        {
            return new AuthResult(false, validationMessage);
        }

        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            return new AuthResult(false, "An account with this email already exists.");
        }

        User user = model.Role switch
        {
            UserRoles.Student => new Student
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHasher.Hash(model.Password),
                Role = UserRoles.Student,
                DateOfBirth = model.DateOfBirth!.Value,
                Phone = model.Phone.Trim(),
                Address = model.Address.Trim()
            },
            UserRoles.Employer => new Employer
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHasher.Hash(model.Password),
                Role = UserRoles.Employer,
                CompanyName = model.CompanyName.Trim(),
                CompanyEmail = string.IsNullOrWhiteSpace(model.CompanyEmail)
                    ? model.Email.Trim().ToLowerInvariant()
                    : model.CompanyEmail.Trim().ToLowerInvariant()
            },
            _ => throw new InvalidOperationException("Only Student and Employer self-registration is supported.")
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (user is Student student && student.StudentId == 0)
        {
            student.StudentId = 2026000 + student.UserId;
            await _db.SaveChangesAsync();
        }

        return new AuthResult(true, "Registration successful.", user.UserId, user.FullName, user.Role);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            return new AuthResult(false, "Invalid email or password.");
        }

        return new AuthResult(true, "Login successful.", user.UserId, user.FullName, user.Role);
    }
}
