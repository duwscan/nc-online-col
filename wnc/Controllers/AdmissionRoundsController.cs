using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Infrastructure.Queries;
using wnc.Models;
using wnc.ViewModels.AdmissionRounds;
using wnc.ViewModels.Shared;

namespace wnc.Controllers;

public class AdmissionRoundsController(AppDbContext context) : Controller
{
    private static readonly IReadOnlyDictionary<string, SortOption<AdmissionRound>> SortOptions =
        new Dictionary<string, SortOption<AdmissionRound>>(StringComparer.OrdinalIgnoreCase)
        {
            ["roundCode"] = new(query => query.OrderBy(x => x.RoundCode), query => query.OrderByDescending(x => x.RoundCode)),
            ["roundName"] = new(query => query.OrderBy(x => x.RoundName), query => query.OrderByDescending(x => x.RoundName)),
            ["year"] = new(query => query.OrderBy(x => x.AdmissionYear), query => query.OrderByDescending(x => x.AdmissionYear)),
            ["startAt"] = new(query => query.OrderBy(x => x.StartAt), query => query.OrderByDescending(x => x.StartAt)),
            ["endAt"] = new(query => query.OrderBy(x => x.EndAt), query => query.OrderByDescending(x => x.EndAt)),
            ["status"] = new(query => query.OrderBy(x => x.Status), query => query.OrderByDescending(x => x.Status)),
            ["updatedAt"] = new(query => query.OrderBy(x => x.UpdatedAt), query => query.OrderByDescending(x => x.UpdatedAt))
        };

    public async Task<IActionResult> Index([FromQuery] QueryPipelineRequest request, CancellationToken cancellationToken)
    {
        var baseQuery = context.AdmissionRounds
            .AsNoTracking()
            .Include(x => x.CreatedByUser);

        var result = await new QueryPipeline<AdmissionRound>(baseQuery)
            .Search(request.Search, (query, term) => query.Where(x =>
                EF.Functions.Like(x.RoundCode, $"%{term}%") ||
                EF.Functions.Like(x.RoundName, $"%{term}%") ||
                EF.Functions.Like(x.Status, $"%{term}%") ||
                x.AdmissionYear.ToString().Contains(term) ||
                (x.CreatedByUser != null && (
                    (x.CreatedByUser.Username != null && EF.Functions.Like(x.CreatedByUser.Username, $"%{term}%")) ||
                    (x.CreatedByUser.Email != null && EF.Functions.Like(x.CreatedByUser.Email, $"%{term}%"))))))
            .Sort(request, "startAt", SortOptions)
            .SelectPageAsync(request, x => new AdmissionRoundListItemViewModel
            {
                Id = x.Id,
                RoundCode = x.RoundCode,
                RoundName = x.RoundName,
                AdmissionYear = x.AdmissionYear,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                Status = x.Status,
                AllowEnrollmentConfirmation = x.AllowEnrollmentConfirmation,
                CreatedByName = x.CreatedByUser != null
                    ? (x.CreatedByUser.Username ?? x.CreatedByUser.Email ?? "Assigned user")
                    : "System",
                UpdatedAt = x.UpdatedAt
            }, cancellationToken);

        return View(new IndexPageViewModel<AdmissionRoundListItemViewModel>
        {
            Title = "Admission Rounds",
            SearchPlaceholder = "Search by code, name, year, owner...",
            Query = request,
            Result = result
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await context.AdmissionRounds
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Where(x => x.Id == id)
            .Select(x => new AdmissionRoundDetailsViewModel
            {
                Id = x.Id,
                RoundCode = x.RoundCode,
                RoundName = x.RoundName,
                AdmissionYear = x.AdmissionYear,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                Status = x.Status,
                Notes = x.Notes,
                AllowEnrollmentConfirmation = x.AllowEnrollmentConfirmation,
                CreatedBy = x.CreatedBy,
                CreatedByName = x.CreatedByUser != null
                    ? (x.CreatedByUser.Username ?? x.CreatedByUser.Email ?? "Assigned user")
                    : "System",
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
        var model = new AdmissionRoundFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdmissionRoundFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateFormAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var now = DateTime.UtcNow;
        var entity = new AdmissionRound
        {
            Id = Guid.NewGuid(),
            RoundCode = model.RoundCode.Trim(),
            RoundName = model.RoundName.Trim(),
            AdmissionYear = model.AdmissionYear,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            Status = model.Status.Trim(),
            Notes = model.Notes?.Trim(),
            AllowEnrollmentConfirmation = model.AllowEnrollmentConfirmation,
            CreatedBy = model.CreatedBy,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.AdmissionRounds.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Admission round created successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.AdmissionRounds
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var model = new AdmissionRoundFormViewModel
        {
            RoundCode = entity.RoundCode,
            RoundName = entity.RoundName,
            AdmissionYear = entity.AdmissionYear,
            StartAt = entity.StartAt,
            EndAt = entity.EndAt,
            Status = entity.Status,
            Notes = entity.Notes,
            AllowEnrollmentConfirmation = entity.AllowEnrollmentConfirmation,
            CreatedBy = entity.CreatedBy
        };

        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdmissionRoundFormViewModel model, CancellationToken cancellationToken)
    {
        var entity = await context.AdmissionRounds.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

        entity.RoundCode = model.RoundCode.Trim();
        entity.RoundName = model.RoundName.Trim();
        entity.AdmissionYear = model.AdmissionYear;
        entity.StartAt = model.StartAt;
        entity.EndAt = model.EndAt;
        entity.Status = model.Status.Trim();
        entity.Notes = model.Notes?.Trim();
        entity.AllowEnrollmentConfirmation = model.AllowEnrollmentConfirmation;
        entity.CreatedBy = model.CreatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Admission round updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await context.AdmissionRounds.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Admission round was not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            context.AdmissionRounds.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Admission round deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this admission round because it is referenced by other records.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateOptionsAsync(AdmissionRoundFormViewModel model, CancellationToken cancellationToken)
    {
        model.CreatedByOptions = await context.Users
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Username ?? x.Email)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Username ?? x.Email ?? "Unnamed user"
            })
            .ToListAsync(cancellationToken);
    }

    private async Task ValidateFormAsync(AdmissionRoundFormViewModel model, Guid? id, CancellationToken cancellationToken)
    {
        if (model.StartAt >= model.EndAt)
        {
            ModelState.AddModelError(nameof(model.EndAt), "End time must be after start time.");
        }

        if (await context.AdmissionRounds.AnyAsync(
                x => x.RoundCode == model.RoundCode.Trim() && x.Id != id,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.RoundCode), "Round code already exists.");
        }

        if (model.CreatedBy.HasValue &&
            !await context.Users.AnyAsync(x => x.Id == model.CreatedBy.Value && x.DeletedAt == null, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.CreatedBy), "Selected user does not exist.");
        }
    }
}
