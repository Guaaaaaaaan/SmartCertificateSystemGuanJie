using Microsoft.AspNetCore.Mvc;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Services;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

public class CertificateController(
    CertificateService certificateService,
    FileService fileService,
    TranscriptPdfService transcriptPdfService,
    ExceptionLogger logger) : Controller
{
    private readonly CertificateService _certificateService = certificateService;
    private readonly FileService _fileService = fileService;
    private readonly TranscriptPdfService _transcriptPdfService = transcriptPdfService;
    private readonly ExceptionLogger _logger = logger;

    [HttpGet]
    public IActionResult Verify() => View(new CertificateVerificationViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(CertificateVerificationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Result = await _certificateService.VerifyCertificate(model.CertificateId, model.StudentName, model.DateOfBirth);
        return View(model);
    }

    public async Task<IActionResult> DownloadTranscript(int id)
    {
        try
        {
            var transcript = await _certificateService.GetTranscriptForValidCertificateAsync(id);
            if (transcript is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(transcript.FilePath) && _fileService.FileExists(transcript.FilePath))
            {
                var storedBytes = _fileService.ReadStoredFile(transcript.FilePath);
                return File(storedBytes, "application/pdf", Path.GetFileName(transcript.FilePath));
            }

            var generatedBytes = _transcriptPdfService.GenerateTranscriptPdf(transcript);
            return File(generatedBytes, "application/pdf", _transcriptPdfService.BuildTranscriptFileName(transcript));
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Verify));
        }
    }
}
