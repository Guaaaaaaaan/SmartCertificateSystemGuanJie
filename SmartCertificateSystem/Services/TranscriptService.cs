using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Services;

public class TranscriptService(AppDbContext db, GpaCalculator gpaCalculator)
{
    private readonly AppDbContext _db = db;
    private readonly GpaCalculator _gpaCalculator = gpaCalculator;

    public async Task<Transcript> GenerateTranscript(int studentId)
    {
        var student = await _db.Students.Include(s => s.Transcripts).FirstOrDefaultAsync(s => s.UserId == studentId)
            ?? throw new InvalidOperationException("Student not found.");

        var transcript = new Transcript
        {
            StudentId = student.UserId,
            GeneratedDate = DateTime.UtcNow,
            Grades =
            [
                CreateGrade("Object-Oriented Design", 88, 4),
                CreateGrade("C# Programming", 82, 4),
                CreateGrade("Database Development", 76, 3)
            ]
        };

        transcript.GPA = _gpaCalculator.CalculateGpa(transcript.Grades);
        student.GPA = transcript.GPA;

        _db.Transcripts.Add(transcript);
        await _db.SaveChangesAsync();

        var latestCertificate = await _db.Certificates
            .Where(c => c.StudentId == student.UserId)
            .OrderByDescending(c => c.IssueDate)
            .FirstOrDefaultAsync();
        if (latestCertificate is not null && latestCertificate.TranscriptId is null)
        {
            latestCertificate.TranscriptId = transcript.TranscriptId;
            await _db.SaveChangesAsync();
        }

        return transcript;
    }

    public async Task<List<Transcript>> GetStudentTranscriptsAsync(int studentId) =>
        await _db.Transcripts
            .Include(t => t.Grades)
            .Where(t => t.StudentId == studentId)
            .OrderByDescending(t => t.GeneratedDate)
            .ToListAsync();

    public async Task DisplayGrades(int transcriptId)
    {
        var transcript = await _db.Transcripts.Include(t => t.Grades).FirstOrDefaultAsync(t => t.TranscriptId == transcriptId)
            ?? throw new InvalidOperationException("Transcript not found.");
        transcript.DisplayGrades();
    }

    private Grade CreateGrade(string moduleName, double score, int credits) => new()
    {
        ModuleName = moduleName,
        Score = score,
        LetterGrade = _gpaCalculator.GetLetterGrade(score),
        CreditValue = credits
    };
}
