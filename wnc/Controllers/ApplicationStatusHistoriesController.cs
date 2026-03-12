using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.ApplicationStatusHistories;
using wnc.Features.Common.Lookups;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Controllers;

public class ApplicationStatusHistoriesController(
    AppDbContext context,
    QueryPipelineService queryPipeline,
    CrudLookupService lookupService) : CrudControllerBase(context)
{
    private static readonly ApplicationStatusHistoryQueryDefinition QueryDefinition = new();

    public async Task<IActionResult> Index(ApplicationStatusHistoryIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.ApplicationStatusHistories, query, QueryDefinition, cancellationToken);

        return View(new ListPageViewModel<ApplicationStatusHistoryIndexQuery, ApplicationStatusHistoryListItem>
        {
            Query = query,
            Results = results
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationStatusHistories
            .AsNoTracking()
            .Include(x => x.Application)
            .Include(x => x.ChangedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ApplicationStatusHistoryFormModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationStatusHistoryFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        Context.ApplicationStatusHistories.Add(MapToEntity(model));

        if (!await TrySaveChangesAsync("Unable to create application status history. Check related records and try again."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Application status history created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationStatusHistories.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var model = new ApplicationStatusHistoryFormModel
        {
            ApplicationId = entity.ApplicationId,
            FromStatus = entity.FromStatus,
            ToStatus = entity.ToStatus,
            ChangedBy = entity.ChangedBy,
            ChangedAt = entity.ChangedAt,
            Reason = entity.Reason,
            PublicNote = entity.PublicNote,
            InternalNote = entity.InternalNote
        };

        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ApplicationStatusHistoryFormModel model, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationStatusHistories.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        entity.ApplicationId = model.ApplicationId;
        entity.FromStatus = model.FromStatus;
        entity.ToStatus = model.ToStatus;
        entity.ChangedBy = model.ChangedBy;
        entity.ChangedAt = model.ChangedAt;
        entity.Reason = model.Reason;
        entity.PublicNote = model.PublicNote;
        entity.InternalNote = model.InternalNote;

        if (!await TrySaveChangesAsync("Unable to update application status history."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Application status history updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationStatusHistories.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await TryDeleteAsync(entity, "Unable to delete application status history."))
        {
            TempData["SuccessMessage"] = "Application status history deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(ApplicationStatusHistoryFormModel model, CancellationToken cancellationToken)
    {
        model.ApplicationOptions = await lookupService.GetApplicationOptionsAsync(cancellationToken: cancellationToken);
        model.UserOptions = await lookupService.GetUserOptionsAsync(includeBlank: true, cancellationToken: cancellationToken);
    }

    private static ApplicationStatusHistory MapToEntity(ApplicationStatusHistoryFormModel model)
    {
        return new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = model.ApplicationId,
            FromStatus = model.FromStatus,
            ToStatus = model.ToStatus,
            ChangedBy = model.ChangedBy,
            ChangedAt = model.ChangedAt,
            Reason = model.Reason,
            PublicNote = model.PublicNote,
            InternalNote = model.InternalNote
        };
    }
}
