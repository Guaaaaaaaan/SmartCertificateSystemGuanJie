using Microsoft.AspNetCore.Mvc;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

public class EmployerController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString(SessionKeys.Role) != UserRoles.Employer)
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }
}
