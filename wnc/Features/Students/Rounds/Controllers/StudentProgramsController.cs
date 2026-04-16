using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;

namespace wnc.Features.Students.Rounds.Controllers;

[Authorize(Roles = "CANDIDATE")]
public class StudentProgramsController(AppDbContext dbContext) : Controller
{
    [HttpGet("/student/programs/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var rp = await dbContext.RoundPrograms
            .Include(x => x.Round)
            .Include(x => x.Program)
            .Include(x => x.Major)
            .Include(x => x.AdmissionMethods)
                .ThenInclude(x => x.Method)
            .Include(x => x.DocumentRequirements)
                .ThenInclude(x => x.DocumentType)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (rp == null)
            return NotFound();

        return View("~/Views/Student/ProgramDetail.cshtml", rp);
    }
}
