using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.RoundPrograms;
using wnc.ViewModels.Shared;

namespace wnc.Controllers;

public class RoundProgramsController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<RoundProgram>> SortOptions =
        new Dictionary<string, SortOption<RoundProgram>>(StringComparer.OrdinalIgnoreCase)
        {
            ["round"] = new(query => query.OrderBy(x => x.Round.RoundName), query => query.OrderByDescending(x => x.Round.RoundName)),
            ["program"] = new(query => query.OrderBy(x => x.Program.ProgramName), query => query.OrderByDescending(x => x.Program.ProgramName)),
            ["major"] = new(query => query.OrderBy(x => x.Major != null ? x.Major.MajorName : string.Empty), query => query.OrderByDescending(x => x.Major != null ? x.Major.MajorName : string.Empty)),
            ["quota"] = new(query => query.OrderBy(x => x.Quota), query => query.OrderByDescending(x => x.Quota)),
            ["publishedQuota"] = new(query => query.OrderBy(x => x.PublishedQuota), query => query.OrderByDescending(x => x.PublishedQuota)),
            ["status"] = new(query => query.OrderBy(x => x.Status), query => query.OrderByDescending(x => x.Status)),
            ["updatedAt"] = new(query => query.OrderBy(x => x.UpdatedAt), query => query.OrderByDescending(x => x.UpdatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var baseQuery = context.RoundPrograms
            .AsNoTracking()
            .Include(x => x.Round)
            .Include(x => x.Program)
            .Include(x => x.Major);

        var result = await new QueryPipeline<RoundProgram>(baseQuery)
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.Round.RoundCode, $"%{term}%") ||
                EF.Functions.Like(x.Round.RoundName, $"%{term}%") ||
                EF.Functions.Like(x.Program.ProgramCode, $"%{term}%") ||
                EF.Functions.Like(x.Program.ProgramName, $"%{term}%") ||
                (x.Major != null && (
                    EF.Functions.Like(x.Major.MajorCode, $"%{term}%") ||
                    EF.Functions.Like(x.Major.MajorName, $"%{term}%"))) ||
                EF.Functions.Like(x.Status, $"%{term}%")))
            .Sort(request, "round", SortOptions)
            .SelectPageAsync(request, x => new RoundProgramListItemViewModel
            {
                Id = x.Id,
                RoundName = x.Round.RoundCode + " - " + x.Round.RoundName,
                ProgramName = x.Program.ProgramCode + " - " + x.Program.ProgramName,
                MajorName = x.Major != null ? x.Major.MajorCode + " - " + x.Major.MajorName : "All majors",
                Quota = x.Quota,
                PublishedQuota = x.PublishedQuota,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<RoundProgramListItemViewModel>
        {
            Title = "Round Programs",
            SearchPlaceholder = "Search by round, program, major...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.RoundPrograms
            .AsNoTracking()
            .Include(x => x.Round)
            .Include(x => x.Program)
            .Include(x => x.Major)
            .Where(x => x.Id == id)
            .Select(x => new RoundProgramDetailsViewModel
            {
                Id = x.Id,
                RoundId = x.RoundId,
                ProgramId = x.ProgramId,
                MajorId = x.MajorId,
                RoundName = x.Round.RoundCode + " - " + x.Round.RoundName,
                ProgramName = x.Program.ProgramCode + " - " + x.Program.ProgramName,
                MajorName = x.Major != null ? x.Major.MajorCode + " - " + x.Major.MajorName : "All majors",
                Quota = x.Quota,
                PublishedQuota = x.PublishedQuota,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new RoundProgramFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoundProgramFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var now = DateTime.UtcNow;
        var entity = new RoundProgram
        {
            Id = Guid.NewGuid(),
            RoundId = model.RoundId!.Value,
            ProgramId = model.ProgramId!.Value,
            MajorId = model.MajorId,
            Quota = model.Quota,
            PublishedQuota = model.PublishedQuota,
            Status = model.Status.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        context.RoundPrograms.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Round program created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.RoundPrograms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var model = new RoundProgramFormViewModel
        {
            RoundId = entity.RoundId,
            ProgramId = entity.ProgramId,
            MajorId = entity.MajorId,
            Quota = entity.Quota,
            PublishedQuota = entity.PublishedQuota,
            Status = entity.Status
        };

        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RoundProgramFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.RoundPrograms.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        await ValidateFormAsync(model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        entity.RoundId = model.RoundId!.Value;
        entity.ProgramId = model.ProgramId!.Value;
        entity.MajorId = model.MajorId;
        entity.Quota = model.Quota;
        entity.PublishedQuota = model.PublishedQuota;
        entity.Status = model.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Round program updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.RoundPrograms.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Round program was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.RoundPrograms.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Round program deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this round program because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateOptionsAsync(RoundProgramFormViewModel model, CancellationToken cancellationToken)
    {
        model.RoundOptions = await context.AdmissionRounds
            .AsNoTracking()
            .OrderByDescending(x => x.AdmissionYear)
            .ThenBy(x => x.StartAt)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.RoundCode + " - " + x.RoundName
            })
            .ToListAsync(cancellationToken);

        model.ProgramOptions = await context.TrainingPrograms
            .AsNoTracking()
            .OrderBy(x => x.ProgramName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ProgramCode + " - " + x.ProgramName
            })
            .ToListAsync(cancellationToken);

        var majorQuery = context.Majors.AsNoTracking();
        if (model.ProgramId.HasValue)
        {
            majorQuery = majorQuery.Where(x => x.ProgramId == model.ProgramId.Value);
        }

        model.MajorOptions = await majorQuery
            .OrderBy(x => x.MajorName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.MajorCode + " - " + x.MajorName
            })
            .ToListAsync(cancellationToken);
    }

    private async Task ValidateFormAsync(RoundProgramFormViewModel model, Guid? id, CancellationToken cancellationToken)
    {
        if (model.RoundId is null || !await context.AdmissionRounds.AnyAsync(x => x.Id == model.RoundId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.RoundId), "Admission round is required.");
        }

        if (model.ProgramId is null || !await context.TrainingPrograms.AnyAsync(x => x.Id == model.ProgramId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.ProgramId), "Training program is required.");
        }

        if (model.MajorId.HasValue)
        {
            var isValidMajor = await context.Majors.AnyAsync(
                x => x.Id == model.MajorId.Value && model.ProgramId.HasValue && x.ProgramId == model.ProgramId.Value,
                cancellationToken);

            if (!isValidMajor)
            {
                ModelState.AddModelError(nameof(model.MajorId), "Major must belong to the selected training program.");
            }
        }

        if (model.RoundId.HasValue && model.ProgramId.HasValue &&
            await context.RoundPrograms.AnyAsync(
                x => x.RoundId == model.RoundId.Value &&
                     x.ProgramId == model.ProgramId.Value &&
                     x.MajorId == model.MajorId &&
                     x.Id != id,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.ProgramId), "This round/program/major combination already exists.");
        }
    }
}
