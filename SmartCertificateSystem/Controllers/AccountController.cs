using Microsoft.AspNetCore.Mvc;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Services;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

public class AccountController(AuthService authService, ExceptionLogger logger) : Controller
{
    private readonly AuthService _authService = authService;
    private readonly ExceptionLogger _logger = logger;

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model.Email, model.Password);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        HttpContext.Session.SetInt32(SessionKeys.UserId, result.UserId!.Value);
        HttpContext.Session.SetString(SessionKeys.FullName, result.FullName!);
        HttpContext.Session.SetString(SessionKeys.Role, result.Role!);

        return result.Role switch
        {
            UserRoles.Admin => RedirectToAction("Index", "Admin"),
            UserRoles.Student => RedirectToAction("Dashboard", "Student"),
            UserRoles.Employer => RedirectToAction("Index", "Employer"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel { Role = UserRoles.Student });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _authService.RegisterAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = "Registration successful. Please log in.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
