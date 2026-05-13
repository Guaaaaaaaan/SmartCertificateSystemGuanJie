using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;

namespace SmartCertificateSystem.Services;

public class SearchSortService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<Student>> SearchStudentByName(string name)
    {
        var students = await _db.Students.ToListAsync();
        return students
            .Where(s => s.FullName.Contains(name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.FullName)
            .ToList();
    }

    public async Task<Certificate?> SearchCertificateById(string certificateId)
    {
        var certificates = await _db.Certificates.Include(c => c.Student).ToListAsync();
        return certificates.FirstOrDefault(c => c.CertificateId.Equals(certificateId.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<Student>> SortStudentsByGpa()
    {
        var students = await _db.Students.ToListAsync();
        return students.OrderByDescending(s => s.GPA).ToList();
    }

    public async Task<List<Student>> SortStudentsAlphabetically()
    {
        var students = await _db.Students.ToListAsync();
        return students.OrderBy(s => s.FullName).ToList();
    }
}
