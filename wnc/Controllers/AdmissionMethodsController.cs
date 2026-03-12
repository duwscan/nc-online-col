using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.AdmissionMethods;
using wnc.ViewModels.Shared;

namespace wnc.Controllers;

public class AdmissionMethodsController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<AdmissionMethod>> SortOptions =
        new Dictionary<string, SortOption<AdmissionMethod>>(StringComparer.OrdinalIgnoreCase)
        {
            ["methodCode"] = new(query => query.OrderBy(x => x.MethodCode), query => query.OrderByDescending(x => x.MethodCode)),
            ["methodName"] = new(query => query.OrderBy(x => x.MethodName), query => query.OrderByDescending(x => x.MethodName)),
            ["status"] = new(query => query.OrderBy(x => x.Status), query => query.OrderByDescending(x => x.Status)),
            ["updatedAt"] = new(query => query.OrderBy(x => x.UpdatedAt), query => query.OrderByDescending(x => x.UpdatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var result = await new QueryPipeline<AdmissionMethod>(context.AdmissionMethods.AsNoTracking())
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.MethodCode, $"%{term}%") ||
                EF.Functions.Like(x.MethodName, $"%{term}%") ||
                (x.Description != null && EF.Functions.Like(x.Description, $"%{term}%")) ||
                EF.Functions.Like(x.Status, $"%{term}%")))
            .Sort(request, "methodName", SortOptions)
            .SelectPageAsync(request, x => new AdmissionMethodListItemViewModel
            {
                Id = x.Id,
                MethodCode = x.MethodCode,
                MethodName = x.MethodName,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<AdmissionMethodListItemViewModel>
        {
            Title = "Admission Methods",
            SearchPlaceholder = "Search by code, name, description...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.AdmissionMethods
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AdmissionMethodDetailsViewModel
            {
                Id = x.Id,
                MethodCode = x.MethodCode,
                MethodName = x.MethodName,
                Description = x.Description,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? NotFound() : View(model);
    }

    public IActionResult Create()
        => View(new AdmissionMethodFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdmissionMethodFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var now = DateTime.UtcNow;
        var entity = new AdmissionMethod
        {
            Id = Guid.NewGuid(),
            MethodCode = model.MethodCode.Trim(),
            MethodName = model.MethodName.Trim(),
            Description = model.Description?.Trim(),
            Status = model.Status.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        context.AdmissionMethods.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Admission method created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.AdmissionMethods
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        return View(new AdmissionMethodFormViewModel
        {
            MethodCode = entity.MethodCode,
            MethodName = entity.MethodName,
            Description = entity.Description,
            Status = entity.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdmissionMethodFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.AdmissionMethods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        await ValidateFormAsync(model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        entity.MethodCode = model.MethodCode.Trim();
        entity.MethodName = model.MethodName.Trim();
        entity.Description = model.Description?.Trim();
        entity.Status = model.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Admission method updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.AdmissionMethods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Admission method was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.AdmissionMethods.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Admission method deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this admission method because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task ValidateFormAsync(AdmissionMethodFormViewModel model, Guid? id, CancellationToken cancellationToken)
    {
        if (await context.AdmissionMethods.AnyAsync(
                x => x.MethodCode == model.MethodCode.Trim() && x.Id != id,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.MethodCode), "Method code already exists.");
        }
    }
}
