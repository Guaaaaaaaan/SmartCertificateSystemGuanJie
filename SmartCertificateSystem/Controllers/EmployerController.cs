using Microsoft.AspNetCore.Mvc;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

[AuthorizeRole(UserRoles.Employer)]
public class EmployerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
