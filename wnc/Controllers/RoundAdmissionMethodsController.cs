using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.RoundAdmissionMethods;
using wnc.ViewModels.Shared;

namespace wnc.Controllers;

public class RoundAdmissionMethodsController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<RoundAdmissionMethod>> SortOptions =
        new Dictionary<string, SortOption<RoundAdmissionMethod>>(StringComparer.OrdinalIgnoreCase)
        {
            ["roundProgram"] = new(query => query.OrderBy(x => x.RoundProgram.Round.RoundName), query => query.OrderByDescending(x => x.RoundProgram.Round.RoundName)),
            ["method"] = new(query => query.OrderBy(x => x.Method.MethodName), query => query.OrderByDescending(x => x.Method.MethodName)),
            ["combinationCode"] = new(query => query.OrderBy(x => x.CombinationCode), query => query.OrderByDescending(x => x.CombinationCode)),
            ["minimumScore"] = new(query => query.OrderBy(x => x.MinimumScore), query => query.OrderByDescending(x => x.MinimumScore)),
            ["status"] = new(query => query.OrderBy(x => x.Status), query => query.OrderByDescending(x => x.Status)),
            ["createdAt"] = new(query => query.OrderBy(x => x.CreatedAt), query => query.OrderByDescending(x => x.CreatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var baseQuery = context.RoundAdmissionMethods
            .AsNoTracking()
            .Include(x => x.Method)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Round)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Program)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Major);

        var result = await new QueryPipeline<RoundAdmissionMethod>(baseQuery)
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.Method.MethodCode, $"%{term}%") ||
                EF.Functions.Like(x.Method.MethodName, $"%{term}%") ||
                (x.CombinationCode != null && EF.Functions.Like(x.CombinationCode, $"%{term}%")) ||
                EF.Functions.Like(x.RoundProgram.Round.RoundCode, $"%{term}%") ||
                EF.Functions.Like(x.RoundProgram.Round.RoundName, $"%{term}%") ||
                EF.Functions.Like(x.RoundProgram.Program.ProgramCode, $"%{term}%") ||
                EF.Functions.Like(x.RoundProgram.Program.ProgramName, $"%{term}%") ||
                (x.RoundProgram.Major != null && (
                    EF.Functions.Like(x.RoundProgram.Major.MajorCode, $"%{term}%") ||
                    EF.Functions.Like(x.RoundProgram.Major.MajorName, $"%{term}%"))) ||
                EF.Functions.Like(x.Status, $"%{term}%")))
            .Sort(request, "createdAt", SortOptions)
            .SelectPageAsync(request, x => new RoundAdmissionMethodListItemViewModel
            {
                Id = x.Id,
                RoundProgramName = x.RoundProgram.Round.RoundCode + " / " +
                                   x.RoundProgram.Program.ProgramCode +
                                   (x.RoundProgram.Major != null ? " / " + x.RoundProgram.Major.MajorCode : " / ALL"),
                MethodName = x.Method.MethodCode + " - " + x.Method.MethodName,
                CombinationCode = x.CombinationCode,
                MinimumScore = x.MinimumScore,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<RoundAdmissionMethodListItemViewModel>
        {
            Title = "Round Admission Methods",
            SearchPlaceholder = "Search by method, round, program, combination...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.RoundAdmissionMethods
            .AsNoTracking()
            .Include(x => x.Method)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Round)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Program)
            .Include(x => x.RoundProgram)
            .ThenInclude(x => x.Major)
            .Where(x => x.Id == id)
            .Select(x => new RoundAdmissionMethodDetailsViewModel
            {
                Id = x.Id,
                RoundProgramId = x.RoundProgramId,
                MethodId = x.MethodId,
                RoundProgramName = x.RoundProgram.Round.RoundCode + " / " +
                                   x.RoundProgram.Program.ProgramCode +
                                   (x.RoundProgram.Major != null ? " / " + x.RoundProgram.Major.MajorCode : " / ALL"),
                MethodName = x.Method.MethodCode + " - " + x.Method.MethodName,
                CombinationCode = x.CombinationCode,
                MinimumScore = x.MinimumScore,
                PriorityPolicy = x.PriorityPolicy,
                CalculationRule = x.CalculationRule,
                Status = x.Status,
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
        var model = new RoundAdmissionMethodFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoundAdmissionMethodFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var entity = new RoundAdmissionMethod
        {
            Id = Guid.NewGuid(),
            RoundProgramId = model.RoundProgramId!.Value,
            MethodId = model.MethodId!.Value,
            CombinationCode = model.CombinationCode?.Trim(),
            MinimumScore = model.MinimumScore,
            PriorityPolicy = model.PriorityPolicy?.Trim(),
            CalculationRule = model.CalculationRule?.Trim(),
            Status = model.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        context.RoundAdmissionMethods.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Round admission method created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.RoundAdmissionMethods
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var model = new RoundAdmissionMethodFormViewModel
        {
            RoundProgramId = entity.RoundProgramId,
            MethodId = entity.MethodId,
            CombinationCode = entity.CombinationCode,
            MinimumScore = entity.MinimumScore,
            PriorityPolicy = entity.PriorityPolicy,
            CalculationRule = entity.CalculationRule,
            Status = entity.Status
        };

        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RoundAdmissionMethodFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.RoundAdmissionMethods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        await ValidateFormAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        entity.RoundProgramId = model.RoundProgramId!.Value;
        entity.MethodId = model.MethodId!.Value;
        entity.CombinationCode = model.CombinationCode?.Trim();
        entity.MinimumScore = model.MinimumScore;
        entity.PriorityPolicy = model.PriorityPolicy?.Trim();
        entity.CalculationRule = model.CalculationRule?.Trim();
        entity.Status = model.Status.Trim();

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Round admission method updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.RoundAdmissionMethods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Round admission method was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.RoundAdmissionMethods.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Round admission method deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this round admission method because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateOptionsAsync(RoundAdmissionMethodFormViewModel model, CancellationToken cancellationToken)
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

        model.MethodOptions = await context.AdmissionMethods
            .AsNoTracking()
            .OrderBy(x => x.MethodName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.MethodCode + " - " + x.MethodName
            })
            .ToListAsync(cancellationToken);
    }

    private async Task ValidateFormAsync(RoundAdmissionMethodFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.RoundProgramId is null || !await context.RoundPrograms.AnyAsync(x => x.Id == model.RoundProgramId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.RoundProgramId), "Round program is required.");
        }

        if (model.MethodId is null || !await context.AdmissionMethods.AnyAsync(x => x.Id == model.MethodId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.MethodId), "Admission method is required.");
        }
    }
}
