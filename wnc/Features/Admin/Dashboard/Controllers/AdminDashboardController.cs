using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wnc.Features.Admin.Dashboard.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER,REPORT_VIEWER")]
public class AdminDashboardController : Controller
{
    [HttpGet("/admin/dashboard")]
    public IActionResult Index()
    {
        return View("~/Features/Admin/Dashboard/Views/Index.cshtml");
    }
}
