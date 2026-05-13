using Microsoft.AspNetCore.Mvc;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Services;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

[AuthorizeRole(UserRoles.Student)]
public class StudentController(StudentService studentService, FileService fileService, ExceptionLogger logger) : Controller
{
    private readonly StudentService _studentService = studentService;
    private readonly FileService _fileService = fileService;
    private readonly ExceptionLogger _logger = logger;

    public async Task<IActionResult> Dashboard()
    {
        var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var student = await _studentService.GetStudentAsync(userId);
        if (student is null) return RedirectToAction("Login", "Account");

        return View(new StudentDashboardViewModel
        {
            Student = student,
            Certificates = student.Certificates,
            Transcripts = student.Transcripts.OrderByDescending(t => t.GeneratedDate).ToList()
        });
    }

    public async Task<IActionResult> DownloadTranscript(int id)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var transcript = await _studentService.GetOwnedTranscriptAsync(userId, id);
            if (transcript?.FilePath is null)
            {
                return NotFound();
            }

            var bytes = _fileService.ReadStoredFile(transcript.FilePath);
            return File(bytes, "application/octet-stream", Path.GetFileName(transcript.FilePath));
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
