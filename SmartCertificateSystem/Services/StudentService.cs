using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Services;

public class StudentService(AppDbContext db) : ISearchable
{
    private readonly AppDbContext _db = db;

    public async Task<List<Student>> GetAllStudentsAsync() =>
        await _db.Students
            .Include(s => s.Certificates)
            .Include(s => s.Transcripts)
            .OrderBy(s => s.FullName)
            .ToListAsync();

    public async Task<Student?> GetStudentAsync(int userId) =>
        await _db.Students
            .Include(s => s.Certificates)
            .Include(s => s.Transcripts).ThenInclude(t => t.Grades)
            .FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<Student> AddStudentAsync(StudentFormViewModel model)
    {
        if (await _db.Users.AnyAsync(u => u.Email == model.Email.Trim().ToLowerInvariant()))
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var student = new Student
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PasswordHash = PasswordHasher.Hash(model.Password ?? "Student123!"),
            Role = UserRoles.Student,
            DateOfBirth = model.DateOfBirth,
            Phone = model.Phone.Trim(),
            Address = model.Address.Trim(),
            GPA = model.GPA
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        student.StudentId = 2026000 + student.UserId;
        await _db.SaveChangesAsync();

        return student;
    }

    public async Task UpdateStudentAsync(StudentFormViewModel model)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == model.UserId)
            ?? throw new InvalidOperationException("Student not found.");

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == normalizedEmail && u.UserId != model.UserId))
        {
            throw new InvalidOperationException("Another user already has this email.");
        }

        student.FullName = model.FullName.Trim();
        student.Email = normalizedEmail;
        student.DateOfBirth = model.DateOfBirth;
        student.Phone = model.Phone.Trim();
        student.Address = model.Address.Trim();
        student.GPA = model.GPA;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            student.PasswordHash = PasswordHasher.Hash(model.Password);
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(int userId)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId)
            ?? throw new InvalidOperationException("Student not found.");

        _db.Students.Remove(student);
        await _db.SaveChangesAsync();
    }

    public List<Student> SearchByName(string name) =>
        _db.Students
            .Where(s => s.FullName.Contains(name))
            .OrderBy(s => s.FullName)
            .ToList();

    public object? SearchById(string id)
    {
        if (!int.TryParse(id, out var userId))
        {
            return null;
        }

        return _db.Students.FirstOrDefault(s => s.UserId == userId || s.StudentId == userId);
    }

    public List<Student> SortByGPA() => _db.Students.OrderByDescending(s => s.GPA).ToList();

    public List<Student> SortAlphabetically() => _db.Students.OrderBy(s => s.FullName).ToList();
}
