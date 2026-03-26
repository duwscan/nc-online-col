using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;

namespace wnc.Features.Admin.Dashboard.Controllers;

[Authorize(Roles = "ADMIN")]
public class AdminDashboardController(AppDbContext dbContext) : Controller
{
    [HttpGet("/admin/dashboard")]
    public async Task<IActionResult> Index()
    {
        var studentCount = await dbContext.Candidates.CountAsync();

        var currentRound = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Where(r => r.Status == "PUBLISHED" && r.DeletedAt == null)
            .Where(r => r.StartAt <= DateTime.UtcNow && r.EndAt >= DateTime.UtcNow)
            .OrderByDescending(r => r.StartAt)
            .FirstOrDefaultAsync();

        var displayName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "Admin";

        ViewData["Title"] = "Bảng điều khiển";
        ViewData["StudentCount"] = studentCount;
        ViewData["CurrentRound"] = currentRound;
        ViewData["DisplayName"] = displayName;

        return View("~/Features/Admin/Dashboard/Views/Index.cshtml");
    }
}
