using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString(SessionKeys.Role);
        return role switch
        {
            UserRoles.Admin => RedirectToAction("Index", "Admin"),
            UserRoles.Student => RedirectToAction("Dashboard", "Student"),
            UserRoles.Employer => RedirectToAction("Index", "Employer"),
            _ => View()
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
