using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.DocumentTypes;
using wnc.ViewModels.Shared;

namespace wnc.Controllers;

public class DocumentTypesController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<DocumentType>> SortOptions =
        new Dictionary<string, SortOption<DocumentType>>(StringComparer.OrdinalIgnoreCase)
        {
            ["documentCode"] = new(query => query.OrderBy(x => x.DocumentCode), query => query.OrderByDescending(x => x.DocumentCode)),
            ["documentName"] = new(query => query.OrderBy(x => x.DocumentName), query => query.OrderByDescending(x => x.DocumentName)),
            ["status"] = new(query => query.OrderBy(x => x.Status), query => query.OrderByDescending(x => x.Status)),
            ["createdAt"] = new(query => query.OrderBy(x => x.CreatedAt), query => query.OrderByDescending(x => x.CreatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var result = await new QueryPipeline<DocumentType>(context.DocumentTypes.AsNoTracking())
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.DocumentCode, $"%{term}%") ||
                EF.Functions.Like(x.DocumentName, $"%{term}%") ||
                (x.Description != null && EF.Functions.Like(x.Description, $"%{term}%")) ||
                EF.Functions.Like(x.Status, $"%{term}%")))
            .Sort(request, "documentName", SortOptions)
            .SelectPageAsync(request, x => new DocumentTypeListItemViewModel
            {
                Id = x.Id,
                DocumentCode = x.DocumentCode,
                DocumentName = x.DocumentName,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<DocumentTypeListItemViewModel>
        {
            Title = "Document Types",
            SearchPlaceholder = "Search by code, name, description...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.DocumentTypes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DocumentTypeDetailsViewModel
            {
                Id = x.Id,
                DocumentCode = x.DocumentCode,
                DocumentName = x.DocumentName,
                Description = x.Description,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? NotFound() : View(model);
    }

    public IActionResult Create()
        => View(new DocumentTypeFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentTypeFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new DocumentType
        {
            Id = Guid.NewGuid(),
            DocumentCode = model.DocumentCode.Trim(),
            DocumentName = model.DocumentName.Trim(),
            Description = model.Description?.Trim(),
            Status = model.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        context.DocumentTypes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Document type created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.DocumentTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        return View(new DocumentTypeFormViewModel
        {
            DocumentCode = entity.DocumentCode,
            DocumentName = entity.DocumentName,
            Description = entity.Description,
            Status = entity.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DocumentTypeFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        await ValidateFormAsync(model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        entity.DocumentCode = model.DocumentCode.Trim();
        entity.DocumentName = model.DocumentName.Trim();
        entity.Description = model.Description?.Trim();
        entity.Status = model.Status.Trim();

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Document type updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Document type was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.DocumentTypes.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Document type deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this document type because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task ValidateFormAsync(DocumentTypeFormViewModel model, Guid? id, CancellationToken cancellationToken)
    {
        if (await context.DocumentTypes.AnyAsync(
                x => x.DocumentCode == model.DocumentCode.Trim() && x.Id != id,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.DocumentCode), "Document code already exists.");
        }
    }
}
