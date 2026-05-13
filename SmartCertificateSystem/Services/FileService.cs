using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Services;

public class FileService
{
    private readonly AppDbContext? _db;
    private readonly InputValidator _validator;
    private readonly string _contentRootPath;

    public FileService(AppDbContext db, IWebHostEnvironment environment, InputValidator validator)
    {
        _db = db;
        _validator = validator;
        _contentRootPath = environment.ContentRootPath;
    }

    public FileService(string contentRootPath, InputValidator validator)
    {
        _validator = validator;
        _contentRootPath = contentRootPath;
    }

    public async Task<string> SaveFile(IFormFile file, string targetFolder)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("The selected file is empty.");
        }

        if (!_validator.IsAllowedFile(file.FileName))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        var folderPath = Path.Combine(_contentRootPath, "FileStorage", targetFolder);
        Directory.CreateDirectory(folderPath);

        var safeName = Path.GetFileNameWithoutExtension(file.FileName)
            .Replace(' ', '_')
            .Replace('/', '_')
            .Replace('\\', '_');
        var extension = Path.GetExtension(file.FileName);
        var finalName = $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var absolutePath = Path.Combine(folderPath, finalName);

        await using var stream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);
        await file.CopyToAsync(stream);

        return Path.Combine("FileStorage", targetFolder, finalName).Replace('\\', '/');
    }

    public byte[] ReadFile(string filePath)
    {
        var absolutePath = GetAbsolutePath(filePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("The requested file is missing.", absolutePath);
        }

        return File.ReadAllBytes(absolutePath);
    }

    public byte[] ReadStoredFile(string filePath)
    {
        var absolutePath = GetSafeStoredAbsolutePath(filePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("The requested file is missing.", absolutePath);
        }

        return File.ReadAllBytes(absolutePath);
    }

    public void DeleteFile(string filePath)
    {
        var absolutePath = GetAbsolutePath(filePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    public bool FileExists(string? filePath) =>
        !string.IsNullOrWhiteSpace(filePath) && File.Exists(GetAbsolutePath(filePath));

    public async Task UploadCertificateFileAsync(int certificateDbId, IFormFile file)
    {
        if (_db is null)
        {
            throw new InvalidOperationException("Database access is required for certificate file uploads.");
        }

        var certificate = await _db.Certificates.FirstOrDefaultAsync(c => c.Id == certificateDbId)
            ?? throw new InvalidOperationException("Certificate not found.");
        certificate.FilePath = await SaveFile(file, "Certificates");
        await _db.SaveChangesAsync();
    }

    public async Task UploadTranscriptFileAsync(int transcriptId, IFormFile file)
    {
        if (_db is null)
        {
            throw new InvalidOperationException("Database access is required for transcript file uploads.");
        }

        var transcript = await _db.Transcripts.FirstOrDefaultAsync(t => t.TranscriptId == transcriptId)
            ?? throw new InvalidOperationException("Transcript not found.");
        transcript.FilePath = await SaveFile(file, "Transcripts");
        await _db.SaveChangesAsync();
    }

    public async Task<string> UploadStudentDocumentAsync(int studentId, IFormFile file)
    {
        if (_db is null)
        {
            throw new InvalidOperationException("Database access is required for student document uploads.");
        }

        if (!await _db.Students.AnyAsync(s => s.UserId == studentId))
        {
            throw new InvalidOperationException("Student not found.");
        }

        return await SaveFile(file, "StudentDocuments");
    }

    public string GetAbsolutePath(string filePath) =>
        Path.IsPathRooted(filePath) ? filePath : Path.Combine(_contentRootPath, filePath);

    private string GetSafeStoredAbsolutePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidOperationException("File path is required.");
        }

        var storageRoot = Path.GetFullPath(Path.Combine(_contentRootPath, "FileStorage"));
        var candidate = Path.GetFullPath(Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(_contentRootPath, filePath));

        if (!candidate.StartsWith(storageRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !candidate.Equals(storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Only files inside FileStorage can be downloaded.");
        }

        return candidate;
    }
}
