using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;

namespace SmartCertificateSystem.Services;

public class CourseService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<Course>> GetCoursesAsync() =>
        await _db.Courses
            .Include(c => c.Modules)
            .Include(c => c.Enrollments).ThenInclude(e => e.Student)
            .OrderBy(c => c.CourseName)
            .ToListAsync();

    public async Task<Course> AddCourse(CourseFormViewModel model)
    {
        var course = new Course
        {
            CourseName = model.CourseName.Trim(),
            Description = model.Description.Trim()
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return course;
    }

    public async Task UpdateCourse(CourseFormViewModel model)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == model.CourseId)
            ?? throw new InvalidOperationException("Course not found.");

        course.CourseName = model.CourseName.Trim();
        course.Description = model.Description.Trim();
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCourse(int courseId)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId)
            ?? throw new InvalidOperationException("Course not found.");

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
    }

    public async Task AddModule(ModuleFormViewModel model)
    {
        if (!await _db.Courses.AnyAsync(c => c.CourseId == model.CourseId))
        {
            throw new InvalidOperationException("Course not found.");
        }

        _db.Modules.Add(new Module
        {
            CourseId = model.CourseId,
            ModuleName = model.ModuleName.Trim(),
            CreditValue = model.CreditValue
        });

        await _db.SaveChangesAsync();
    }

    public async Task UpdateModule(ModuleFormViewModel model)
    {
        var module = await _db.Modules.FirstOrDefaultAsync(m => m.ModuleId == model.ModuleId)
            ?? throw new InvalidOperationException("Module not found.");

        if (!await _db.Courses.AnyAsync(c => c.CourseId == model.CourseId))
        {
            throw new InvalidOperationException("Course not found.");
        }

        module.CourseId = model.CourseId;
        module.ModuleName = model.ModuleName.Trim();
        module.CreditValue = model.CreditValue;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteModule(int moduleId)
    {
        var module = await _db.Modules.FirstOrDefaultAsync(m => m.ModuleId == moduleId)
            ?? throw new InvalidOperationException("Module not found.");

        _db.Modules.Remove(module);
        await _db.SaveChangesAsync();
    }

    public async Task EnrollStudent(EnrollmentFormViewModel model)
    {
        if (!await _db.Students.AnyAsync(s => s.UserId == model.StudentId))
        {
            throw new InvalidOperationException("Student not found.");
        }

        if (!await _db.Courses.AnyAsync(c => c.CourseId == model.CourseId))
        {
            throw new InvalidOperationException("Course not found.");
        }

        if (await _db.Enrollments.AnyAsync(e => e.StudentId == model.StudentId && e.CourseId == model.CourseId))
        {
            throw new InvalidOperationException("Student is already enrolled in this course.");
        }

        _db.Enrollments.Add(new Enrollment
        {
            StudentId = model.StudentId,
            CourseId = model.CourseId
        });

        await _db.SaveChangesAsync();
    }
}
