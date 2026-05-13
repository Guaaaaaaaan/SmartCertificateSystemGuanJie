using Microsoft.AspNetCore.Mvc;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Services;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

public class CertificateController(CertificateService certificateService, FileService fileService, ExceptionLogger logger) : Controller
{
    private readonly CertificateService _certificateService = certificateService;
    private readonly FileService _fileService = fileService;
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

    public async Task<IActionResult> DownloadTranscript(string path)
    {
        try
        {
            var bytes = _fileService.ReadFile(path);
            return File(bytes, "application/octet-stream", Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Verify));
        }
    }
}
