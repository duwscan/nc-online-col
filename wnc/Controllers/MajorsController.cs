using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.Majors;
using wnc.ViewModels.Shared;

namespace wnc.Controllers;

public class MajorsController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<Major>> SortOptions =
        new Dictionary<string, SortOption<Major>>(StringComparer.OrdinalIgnoreCase)
        {
            ["majorCode"] = new(query => query.OrderBy(x => x.MajorCode), query => query.OrderByDescending(x => x.MajorCode)),
            ["majorName"] = new(query => query.OrderBy(x => x.MajorName), query => query.OrderByDescending(x => x.MajorName)),
            ["program"] = new(query => query.OrderBy(x => x.Program.ProgramName), query => query.OrderByDescending(x => x.Program.ProgramName)),
            ["quota"] = new(query => query.OrderBy(x => x.Quota), query => query.OrderByDescending(x => x.Quota)),
            ["displayOrder"] = new(query => query.OrderBy(x => x.DisplayOrder), query => query.OrderByDescending(x => x.DisplayOrder)),
            ["status"] = new(query => query.OrderBy(x => x.Status), query => query.OrderByDescending(x => x.Status)),
            ["updatedAt"] = new(query => query.OrderBy(x => x.UpdatedAt), query => query.OrderByDescending(x => x.UpdatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var baseQuery = context.Majors
            .AsNoTracking()
            .Include(x => x.Program);

        var result = await new QueryPipeline<Major>(baseQuery)
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.MajorCode, $"%{term}%") ||
                EF.Functions.Like(x.MajorName, $"%{term}%") ||
                EF.Functions.Like(x.Program.ProgramCode, $"%{term}%") ||
                EF.Functions.Like(x.Program.ProgramName, $"%{term}%") ||
                EF.Functions.Like(x.Status, $"%{term}%")))
            .Sort(request, "displayOrder", SortOptions)
            .SelectPageAsync(request, x => new MajorListItemViewModel
            {
                Id = x.Id,
                MajorCode = x.MajorCode,
                MajorName = x.MajorName,
                ProgramName = x.Program.ProgramCode + " - " + x.Program.ProgramName,
                Quota = x.Quota,
                DisplayOrder = x.DisplayOrder,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<MajorListItemViewModel>
        {
            Title = "Majors",
            SearchPlaceholder = "Search by code, name, program...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.Majors
            .AsNoTracking()
            .Include(x => x.Program)
            .Where(x => x.Id == id)
            .Select(x => new MajorDetailsViewModel
            {
                Id = x.Id,
                ProgramId = x.ProgramId,
                ProgramName = x.Program.ProgramCode + " - " + x.Program.ProgramName,
                MajorCode = x.MajorCode,
                MajorName = x.MajorName,
                Description = x.Description,
                Quota = x.Quota,
                DisplayOrder = x.DisplayOrder,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new MajorFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MajorFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var now = DateTime.UtcNow;
        var entity = new Major
        {
            Id = Guid.NewGuid(),
            ProgramId = model.ProgramId!.Value,
            MajorCode = model.MajorCode.Trim(),
            MajorName = model.MajorName.Trim(),
            Description = model.Description?.Trim(),
            Quota = model.Quota,
            DisplayOrder = model.DisplayOrder,
            Status = model.Status.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Majors.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Major created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.Majors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var model = new MajorFormViewModel
        {
            ProgramId = entity.ProgramId,
            MajorCode = entity.MajorCode,
            MajorName = entity.MajorName,
            Description = entity.Description,
            Quota = entity.Quota,
            DisplayOrder = entity.DisplayOrder,
            Status = entity.Status
        };

        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MajorFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.Majors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

        entity.ProgramId = model.ProgramId!.Value;
        entity.MajorCode = model.MajorCode.Trim();
        entity.MajorName = model.MajorName.Trim();
        entity.Description = model.Description?.Trim();
        entity.Quota = model.Quota;
        entity.DisplayOrder = model.DisplayOrder;
        entity.Status = model.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Major updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.Majors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Major was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.Majors.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Major deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this major because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateOptionsAsync(MajorFormViewModel model, CancellationToken cancellationToken)
    {
        model.ProgramOptions = await context.TrainingPrograms
            .AsNoTracking()
            .OrderBy(x => x.ProgramName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ProgramCode + " - " + x.ProgramName
            })
            .ToListAsync(cancellationToken);
    }

    private async Task ValidateFormAsync(MajorFormViewModel model, Guid? id, CancellationToken cancellationToken)
    {
        if (model.ProgramId is null || !await context.TrainingPrograms.AnyAsync(x => x.Id == model.ProgramId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.ProgramId), "Training program is required.");
        }

        if (await context.Majors.AnyAsync(x => x.MajorCode == model.MajorCode.Trim() && x.Id != id, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.MajorCode), "Major code already exists.");
        }
    }
}
