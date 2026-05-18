using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Services;

public class CertificateService(AppDbContext db, FileService fileService, InputValidator validator, RawSqlHelper rawSqlHelper)
{
    private readonly AppDbContext _db = db;
    private readonly FileService _fileService = fileService;
    private readonly InputValidator _validator = validator;
    private readonly RawSqlHelper _rawSqlHelper = rawSqlHelper;

    public async Task<VerificationResult> VerifyCertificate(int certId)
    {
        var certificate = await _db.Certificates
            .Include(c => c.Student)
            .Include(c => c.Transcript)
            .FirstOrDefaultAsync(c => c.Id == certId);

        if (certificate is null)
        {
            return new VerificationResult(false, "Certificate not found.", null, null, null);
        }

        return BuildResult(certificate, certificate.Student, validateIdentity: false);
    }

    public async Task<VerificationResult> VerifyCertificate(string certId, string studentName)
    {
        var certificate = await _db.Certificates
            .Include(c => c.Student)
            .Include(c => c.Transcript)
            .FirstOrDefaultAsync(c => c.CertificateId == certId.Trim());

        if (certificate is null)
        {
            return new VerificationResult(false, "Certificate not found.", null, null, null);
        }

        if (!MatchesName(certificate.Student, studentName))
        {
            return new VerificationResult(false, "Student details mismatch.", null, null, null);
        }

        return BuildResult(certificate, certificate.Student, validateIdentity: true);
    }

    public async Task<VerificationResult> VerifyCertificate(string certId, string studentName, DateTime dob)
    {
        if (!_validator.ValidateCertificateInput(certId, studentName, dob, out var validationMessage))
        {
            return new VerificationResult(false, validationMessage, null, null, null);
        }

        var certificate = await _db.Certificates
            .Include(c => c.Student)
            .Include(c => c.Transcript)
            .FirstOrDefaultAsync(c => c.CertificateId == certId.Trim());

        if (certificate is null)
        {
            return new VerificationResult(false, "Certificate not found.", null, null, null);
        }

        var rawSqlStatus = await _rawSqlHelper.FindCertificateStatusByIdAsync(certId.Trim());
        if (rawSqlStatus is not null && rawSqlStatus != certificate.Status)
        {
            certificate.Status = rawSqlStatus;
        }

        if (!MatchesName(certificate.Student, studentName) || certificate.Student?.DateOfBirth.Date != dob.Date)
        {
            return new VerificationResult(false, "Student details mismatch.", null, null, null);
        }

        return BuildResult(certificate, certificate.Student, validateIdentity: true);
    }

    public async Task<List<Certificate>> GetCertificatesAsync() =>
        await _db.Certificates.Include(c => c.Student).Include(c => c.Transcript).OrderBy(c => c.CertificateId).ToListAsync();

    public async Task<Certificate> CreateCertificateAsync(int studentUserId, string certificateId, string awardTitle, DateTime completionDate)
    {
        if (await _db.Certificates.AnyAsync(c => c.CertificateId == certificateId.Trim()))
        {
            throw new InvalidOperationException("A certificate with this ID already exists.");
        }

        if (!await _db.Students.AnyAsync(s => s.UserId == studentUserId))
        {
            throw new InvalidOperationException("Student not found.");
        }

        var certificate = new Certificate
        {
            StudentId = studentUserId,
            CertificateId = certificateId.Trim(),
            AwardTitle = awardTitle.Trim(),
            IssueDate = DateTime.Today,
            CompletionDate = completionDate,
            Status = CertificateStatuses.Valid
        };

        _db.Certificates.Add(certificate);
        await _db.SaveChangesAsync();

        return certificate;
    }

    public async Task<Certificate?> SearchCertificateByIdAsync(string certificateId) =>
        await _db.Certificates
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.CertificateId == certificateId.Trim());

    public async Task<Transcript?> GetTranscriptForValidCertificateAsync(int transcriptId) =>
        await _db.Transcripts
            .Include(t => t.Student)
            .Include(t => t.Grades)
            .FirstOrDefaultAsync(t =>
                t.TranscriptId == transcriptId &&
                _db.Certificates.Any(c => c.TranscriptId == t.TranscriptId && c.Status == CertificateStatuses.Valid));

    private VerificationResult BuildResult(Certificate certificate, Student? student, bool validateIdentity)
    {
        if (student is null && validateIdentity)
        {
            return new VerificationResult(false, "Certificate owner record is missing.", null, null, null);
        }

        if (!certificate.IsValid())
        {
            return new VerificationResult(false, $"Certificate is {certificate.Status}.", null, null, null);
        }

        var transcript = certificate.Transcript;
        var transcriptAvailable = transcript is not null;
        var storedTranscriptAvailable = _fileService.FileExists(transcript?.FilePath);
        var message = transcript switch
        {
            null => "Certificate valid, but no transcript record is linked.",
            _ when storedTranscriptAvailable => "Certificate valid. Uploaded transcript access is available.",
            _ => "Certificate valid. Transcript will be generated on demand."
        };

        return new VerificationResult(
            true,
            message,
            certificate.AwardTitle,
            certificate.CompletionDate,
            storedTranscriptAvailable ? transcript!.FilePath : null,
            transcriptAvailable ? transcript!.TranscriptId : null);
    }

    private static bool MatchesName(Student? student, string studentName) =>
        student is not null && student.FullName.Equals(studentName.Trim(), StringComparison.OrdinalIgnoreCase);
}
