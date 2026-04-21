using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Models;

namespace wnc.Features.Students.Rounds.Controllers;

public class RoundsViewModel
{
    public List<AdmissionRound> ActiveRounds { get; set; } = [];
    public List<AdmissionRound> UpcomingRounds { get; set; } = [];
}

[Authorize(Roles = "CANDIDATE")]
public class StudentRoundsController(AppDbContext dbContext) : Controller
{
    [HttpGet("/student/rounds")]
    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var rounds = await dbContext.AdmissionRounds
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Major)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.AdmissionMethods)
                    .ThenInclude(ram => ram.Method)
            .Where(r => r.Status == "PUBLISHED")
            .OrderBy(r => r.StartAt)
            .AsNoTracking()
            .ToListAsync();

        var vm = new RoundsViewModel
        {
            ActiveRounds = rounds.Where(r => r.StartAt <= now && r.EndAt >= now).ToList(),
            UpcomingRounds = rounds.Where(r => r.StartAt > now).ToList()
        };

        return View("~/Views/Student/Rounds.cshtml", vm);
    }
}
