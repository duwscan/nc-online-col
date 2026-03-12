using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.RoundDocumentRequirements;
using wnc.ViewModels.Shared;

namespace wnc.Controllers;

public class RoundDocumentRequirementsController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<RoundDocumentRequirement>> SortOptions =
        new Dictionary<string, SortOption<RoundDocumentRequirement>>(StringComparer.OrdinalIgnoreCase)
        {
            ["roundProgram"] = new(query => query.OrderBy(x => x.RoundProgram.Round.RoundName), query => query.OrderByDescending(x => x.RoundProgram.Round.RoundName)),
            ["documentType"] = new(query => query.OrderBy(x => x.DocumentType.DocumentName), query => query.OrderByDescending(x => x.DocumentType.DocumentName)),
            ["isRequired"] = new(query => query.OrderBy(x => x.IsRequired), query => query.OrderByDescending(x => x.IsRequired)),
            ["maxFiles"] = new(query => query.OrderBy(x => x.MaxFiles), query => query.OrderByDescending(x => x.MaxFiles)),
            ["createdAt"] = new(query => query.OrderBy(x => x.CreatedAt), query => query.OrderByDescending(x => x.CreatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var baseQuery = context.RoundDocumentRequirements
            .AsNoTracking()
            .Include(x => x.DocumentType)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Round)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Program)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Major);

        var result = await new QueryPipeline<RoundDocumentRequirement>(baseQuery)
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.DocumentType.DocumentCode, $"%{term}%") ||
                EF.Functions.Like(x.DocumentType.DocumentName, $"%{term}%") ||
                EF.Functions.Like(x.RoundProgram.Round.RoundCode, $"%{term}%") ||
                EF.Functions.Like(x.RoundProgram.Round.RoundName, $"%{term}%") ||
                EF.Functions.Like(x.RoundProgram.Program.ProgramCode, $"%{term}%") ||
                EF.Functions.Like(x.RoundProgram.Program.ProgramName, $"%{term}%") ||
                (x.RoundProgram.Major != null && (
                    EF.Functions.Like(x.RoundProgram.Major.MajorCode, $"%{term}%") ||
                    EF.Functions.Like(x.RoundProgram.Major.MajorName, $"%{term}%"))) ||
                (x.Notes != null && EF.Functions.Like(x.Notes, $"%{term}%"))))
            .Sort(request, "createdAt", SortOptions)
            .SelectPageAsync(request, x => new RoundDocumentRequirementListItemViewModel
            {
                Id = x.Id,
                RoundProgramName = x.RoundProgram.Round.RoundCode + " / " +
                                   x.RoundProgram.Program.ProgramCode +
                                   (x.RoundProgram.Major != null ? " / " + x.RoundProgram.Major.MajorCode : " / ALL"),
                DocumentTypeName = x.DocumentType.DocumentCode + " - " + x.DocumentType.DocumentName,
                IsRequired = x.IsRequired,
                RequiresNotarization = x.RequiresNotarization,
                RequiresOriginalCopy = x.RequiresOriginalCopy,
                MaxFiles = x.MaxFiles,
                CreatedAt = x.CreatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<RoundDocumentRequirementListItemViewModel>
        {
            Title = "Round Document Requirements",
            SearchPlaceholder = "Search by round, program, document type...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.RoundDocumentRequirements
            .AsNoTracking()
            .Include(x => x.DocumentType)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Round)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Program)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Major)
            .Where(x => x.Id == id)
            .Select(x => new RoundDocumentRequirementDetailsViewModel
            {
                Id = x.Id,
                RoundProgramId = x.RoundProgramId,
                DocumentTypeId = x.DocumentTypeId,
                RoundProgramName = x.RoundProgram.Round.RoundCode + " / " +
                                   x.RoundProgram.Program.ProgramCode +
                                   (x.RoundProgram.Major != null ? " / " + x.RoundProgram.Major.MajorCode : " / ALL"),
                DocumentTypeName = x.DocumentType.DocumentCode + " - " + x.DocumentType.DocumentName,
                IsRequired = x.IsRequired,
                RequiresNotarization = x.RequiresNotarization,
                RequiresOriginalCopy = x.RequiresOriginalCopy,
                MaxFiles = x.MaxFiles,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
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
        var model = new RoundDocumentRequirementFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoundDocumentRequirementFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var entity = new RoundDocumentRequirement
        {
            Id = Guid.NewGuid(),
            RoundProgramId = model.RoundProgramId!.Value,
            DocumentTypeId = model.DocumentTypeId!.Value,
            IsRequired = model.IsRequired,
            RequiresNotarization = model.RequiresNotarization,
            RequiresOriginalCopy = model.RequiresOriginalCopy,
            MaxFiles = model.MaxFiles,
            Notes = model.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        context.RoundDocumentRequirements.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Round document requirement created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.RoundDocumentRequirements
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var model = new RoundDocumentRequirementFormViewModel
        {
            RoundProgramId = entity.RoundProgramId,
            DocumentTypeId = entity.DocumentTypeId,
            IsRequired = entity.IsRequired,
            RequiresNotarization = entity.RequiresNotarization,
            RequiresOriginalCopy = entity.RequiresOriginalCopy,
            MaxFiles = entity.MaxFiles,
            Notes = entity.Notes
        };

        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RoundDocumentRequirementFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.RoundDocumentRequirements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

        entity.RoundProgramId = model.RoundProgramId!.Value;
        entity.DocumentTypeId = model.DocumentTypeId!.Value;
        entity.IsRequired = model.IsRequired;
        entity.RequiresNotarization = model.RequiresNotarization;
        entity.RequiresOriginalCopy = model.RequiresOriginalCopy;
        entity.MaxFiles = model.MaxFiles;
        entity.Notes = model.Notes?.Trim();

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Round document requirement updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.RoundDocumentRequirements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Round document requirement was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.RoundDocumentRequirements.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Round document requirement deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this round document requirement because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateOptionsAsync(RoundDocumentRequirementFormViewModel model, CancellationToken cancellationToken)
    {
        model.RoundProgramOptions = await context.RoundPrograms
            .AsNoTracking()
            .Include(x => x.Round)
            .Include(x => x.Program)
            .Include(x => x.Major)
            .OrderBy(x => x.Round.RoundName)
            .ThenBy(x => x.Program.ProgramName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Round.RoundCode + " / " + x.Program.ProgramCode + (x.Major != null ? " / " + x.Major.MajorCode : " / ALL")
            })
            .ToListAsync(cancellationToken);

        model.DocumentTypeOptions = await context.DocumentTypes
            .AsNoTracking()
            .OrderBy(x => x.DocumentName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.DocumentCode + " - " + x.DocumentName
            })
            .ToListAsync(cancellationToken);
    }

    private async Task ValidateFormAsync(RoundDocumentRequirementFormViewModel model, Guid? id, CancellationToken cancellationToken)
    {
        if (model.RoundProgramId is null || !await context.RoundPrograms.AnyAsync(x => x.Id == model.RoundProgramId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.RoundProgramId), "Round program is required.");
        }

        if (model.DocumentTypeId is null || !await context.DocumentTypes.AnyAsync(x => x.Id == model.DocumentTypeId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.DocumentTypeId), "Document type is required.");
        }

        if (model.RoundProgramId.HasValue && model.DocumentTypeId.HasValue &&
            await context.RoundDocumentRequirements.AnyAsync(
                x => x.RoundProgramId == model.RoundProgramId.Value &&
                     x.DocumentTypeId == model.DocumentTypeId.Value &&
                     x.Id != id,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.DocumentTypeId), "This document type is already configured for the selected round program.");
        }
    }
}
