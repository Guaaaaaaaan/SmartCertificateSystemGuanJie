using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Database;
using SmartCertificateSystem.Models;
using SmartCertificateSystem.Models.ViewModels;
using SmartCertificateSystem.Services;
using SmartCertificateSystem.Utilities;

namespace SmartCertificateSystem.Controllers;

public class AdminController(
    AppDbContext db,
    StudentService studentService,
    CourseService courseService,
    CertificateService certificateService,
    TranscriptService transcriptService,
    FileService fileService,
    SearchSortService searchSortService,
    RawSqlHelper rawSqlHelper,
    ExceptionLogger logger) : Controller
{
    private readonly AppDbContext _db = db;
    private readonly StudentService _studentService = studentService;
    private readonly CourseService _courseService = courseService;
    private readonly CertificateService _certificateService = certificateService;
    private readonly TranscriptService _transcriptService = transcriptService;
    private readonly FileService _fileService = fileService;
    private readonly SearchSortService _searchSortService = searchSortService;
    private readonly RawSqlHelper _rawSqlHelper = rawSqlHelper;
    private readonly ExceptionLogger _logger = logger;

    public async Task<IActionResult> Index()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var model = new AdminDashboardViewModel
        {
            StudentCount = await _db.Students.CountAsync(),
            CertificateCount = await _db.Certificates.CountAsync(),
            CourseCount = await _db.Courses.CountAsync(),
            RawSqlStudentCount = await _rawSqlHelper.CountUsersByRoleAsync(UserRoles.Student)
        };

        return View(model);
    }

    public async Task<IActionResult> Students(string? search, string sort = "name")
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        List<Student> students;
        if (!string.IsNullOrWhiteSpace(search))
        {
            students = await _searchSortService.SearchStudentByName(search);
        }
        else if (sort == "gpa")
        {
            students = await _searchSortService.SortStudentsByGpa();
        }
        else
        {
            students = await _searchSortService.SortStudentsAlphabetically();
        }

        return View(new StudentsIndexViewModel { Search = search, Sort = sort, Students = students });
    }

    [HttpGet]
    public IActionResult CreateStudent()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        return View(new StudentFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudent(StudentFormViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _studentService.AddStudentAsync(model);
            TempData["Success"] = "Student added.";
            return RedirectToAction(nameof(Students));
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditStudent(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var student = await _studentService.GetStudentAsync(id);
        if (student is null) return NotFound();

        return View(new StudentFormViewModel
        {
            UserId = student.UserId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            Phone = student.Phone,
            Address = student.Address,
            GPA = student.GPA
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStudent(StudentFormViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _studentService.UpdateStudentAsync(model);
            TempData["Success"] = "Student updated.";
            return RedirectToAction(nameof(Students));
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
    public async Task<IActionResult> DeleteStudent(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        try
        {
            await _studentService.DeleteStudentAsync(id);
            TempData["Success"] = "Student deleted.";
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Students));
    }

    public async Task<IActionResult> Courses()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        return View(new CourseManagementViewModel
        {
            Courses = await _courseService.GetCoursesAsync(),
            Students = await _studentService.GetAllStudentsAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCourse(CourseFormViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        try
        {
            if (ModelState.IsValid) await _courseService.AddCourse(model);
            TempData["Success"] = "Course added.";
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModule(ModuleFormViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        try
        {
            if (ModelState.IsValid) await _courseService.AddModule(model);
            TempData["Success"] = "Module assigned.";
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnrollStudent(EnrollmentFormViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        try
        {
            if (ModelState.IsValid) await _courseService.EnrollStudent(model);
            TempData["Success"] = "Student enrolled.";
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Courses));
    }

    public async Task<IActionResult> Certificates()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        return View(new CertificatesIndexViewModel
        {
            Certificates = await _certificateService.GetCertificatesAsync(),
            CreateForm = new CertificateCreateViewModel { Students = await BuildStudentOptions() }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCertificate(CertificateCreateViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        try
        {
            if (ModelState.IsValid)
            {
                await _certificateService.CreateCertificateAsync(model.StudentId, model.CertificateId, model.AwardTitle, model.CompletionDate);
                TempData["Success"] = "Certificate created.";
            }
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Certificates));
    }

    public async Task<IActionResult> Files()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        return View(await BuildFileUploadModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadFile(FileUploadViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        try
        {
            if (model.File is null)
            {
                throw new InvalidOperationException("Please choose a file.");
            }

            switch (model.Category)
            {
                case "Certificate":
                    if (model.CertificateId is null) throw new InvalidOperationException("Select a certificate.");
                    await _fileService.UploadCertificateFileAsync(model.CertificateId.Value, model.File);
                    break;
                case "Transcript":
                    if (model.TranscriptId is null) throw new InvalidOperationException("Select a transcript.");
                    await _fileService.UploadTranscriptFileAsync(model.TranscriptId.Value, model.File);
                    break;
                default:
                    await _fileService.UploadStudentDocumentAsync(model.StudentId, model.File);
                    break;
            }

            TempData["Success"] = "File uploaded.";
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Files));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateTranscript(int studentId)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        try
        {
            await _transcriptService.GenerateTranscript(studentId);
            TempData["Success"] = "Transcript generated and GPA updated.";
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Students));
    }

    private bool IsAdmin() => HttpContext.Session.GetString(SessionKeys.Role) == UserRoles.Admin;

    private async Task<FileUploadViewModel> BuildFileUploadModel() => new()
    {
        Students = await BuildStudentOptions(),
        Certificates = await _db.Certificates
            .OrderBy(c => c.CertificateId)
            .Select(c => new SelectListItem($"{c.CertificateId} - {c.AwardTitle}", c.Id.ToString()))
            .ToListAsync(),
        Transcripts = await _db.Transcripts
            .Include(t => t.Student)
            .OrderByDescending(t => t.GeneratedDate)
            .Select(t => new SelectListItem($"{t.Student!.FullName} - {t.GeneratedDate:yyyy-MM-dd}", t.TranscriptId.ToString()))
            .ToListAsync()
    };

    private async Task<List<SelectListItem>> BuildStudentOptions() =>
        await _db.Students
            .OrderBy(s => s.FullName)
            .Select(s => new SelectListItem($"{s.FullName} ({s.StudentId})", s.UserId.ToString()))
            .ToListAsync();
}
