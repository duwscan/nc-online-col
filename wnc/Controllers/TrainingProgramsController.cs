using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.Shared;
using wnc.ViewModels.TrainingPrograms;

namespace wnc.Controllers;

public class TrainingProgramsController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<TrainingProgram>> SortOptions =
        new Dictionary<string, SortOption<TrainingProgram>>(StringComparer.OrdinalIgnoreCase)
        {
            ["programCode"] = new(query => query.OrderBy(x => x.ProgramCode), query => query.OrderByDescending(x => x.ProgramCode)),
            ["programName"] = new(query => query.OrderBy(x => x.ProgramName), query => query.OrderByDescending(x => x.ProgramName)),
            ["quota"] = new(query => query.OrderBy(x => x.Quota), query => query.OrderByDescending(x => x.Quota)),
            ["status"] = new(query => query.OrderBy(x => x.Status), query => query.OrderByDescending(x => x.Status)),
            ["displayOrder"] = new(query => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.ProgramName), query => query.OrderByDescending(x => x.DisplayOrder).ThenByDescending(x => x.ProgramName)),
            ["updatedAt"] = new(query => query.OrderBy(x => x.UpdatedAt), query => query.OrderByDescending(x => x.UpdatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var result = await new QueryPipeline<TrainingProgram>(context.TrainingPrograms.AsNoTracking())
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.ProgramCode, $"%{term}%") ||
                EF.Functions.Like(x.ProgramName, $"%{term}%") ||
                EF.Functions.Like(x.EducationType, $"%{term}%") ||
                (x.ManagingUnit != null && EF.Functions.Like(x.ManagingUnit, $"%{term}%")) ||
                EF.Functions.Like(x.Status, $"%{term}%")))
            .Sort(request, "displayOrder", SortOptions)
            .SelectPageAsync(request, x => new TrainingProgramListItemViewModel
            {
                Id = x.Id,
                ProgramCode = x.ProgramCode,
                ProgramName = x.ProgramName,
                EducationType = x.EducationType,
                Quota = x.Quota,
                Status = x.Status,
                DisplayOrder = x.DisplayOrder,
                UpdatedAt = x.UpdatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<TrainingProgramListItemViewModel>
        {
            Title = "Training Programs",
            SearchPlaceholder = "Search by code, name, education type, unit...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.TrainingPrograms
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new TrainingProgramDetailsViewModel
            {
                Id = x.Id,
                ProgramCode = x.ProgramCode,
                ProgramName = x.ProgramName,
                EducationType = x.EducationType,
                Description = x.Description,
                TuitionFee = x.TuitionFee,
                DurationText = x.DurationText,
                Quota = x.Quota,
                ManagingUnit = x.ManagingUnit,
                Status = x.Status,
                DisplayOrder = x.DisplayOrder,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? NotFound() : View(model);
    }

    public IActionResult Create()
        => View(new TrainingProgramFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainingProgramFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var now = DateTime.UtcNow;
        var entity = new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = model.ProgramCode.Trim(),
            ProgramName = model.ProgramName.Trim(),
            EducationType = model.EducationType.Trim(),
            Description = model.Description?.Trim(),
            TuitionFee = model.TuitionFee,
            DurationText = model.DurationText?.Trim(),
            Quota = model.Quota,
            ManagingUnit = model.ManagingUnit?.Trim(),
            Status = model.Status.Trim(),
            DisplayOrder = model.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.TrainingPrograms.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Training program created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.TrainingPrograms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        return View(new TrainingProgramFormViewModel
        {
            ProgramCode = entity.ProgramCode,
            ProgramName = entity.ProgramName,
            EducationType = entity.EducationType,
            Description = entity.Description,
            TuitionFee = entity.TuitionFee,
            DurationText = entity.DurationText,
            Quota = entity.Quota,
            ManagingUnit = entity.ManagingUnit,
            Status = entity.Status,
            DisplayOrder = entity.DisplayOrder
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TrainingProgramFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.TrainingPrograms.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        await ValidateFormAsync(model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        entity.ProgramCode = model.ProgramCode.Trim();
        entity.ProgramName = model.ProgramName.Trim();
        entity.EducationType = model.EducationType.Trim();
        entity.Description = model.Description?.Trim();
        entity.TuitionFee = model.TuitionFee;
        entity.DurationText = model.DurationText?.Trim();
        entity.Quota = model.Quota;
        entity.ManagingUnit = model.ManagingUnit?.Trim();
        entity.Status = model.Status.Trim();
        entity.DisplayOrder = model.DisplayOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Training program updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.TrainingPrograms.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Training program was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.TrainingPrograms.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Training program deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this training program because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task ValidateFormAsync(TrainingProgramFormViewModel model, Guid? id, CancellationToken cancellationToken)
    {
        if (await context.TrainingPrograms.AnyAsync(
                x => x.ProgramCode == model.ProgramCode.Trim() && x.Id != id,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.ProgramCode), "Program code already exists.");
        }
    }
}
