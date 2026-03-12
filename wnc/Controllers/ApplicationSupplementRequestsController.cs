using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.ApplicationSupplementRequests;
using wnc.Features.Common.Lookups;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Controllers;

public class ApplicationSupplementRequestsController(
    AppDbContext context,
    QueryPipelineService queryPipeline,
    CrudLookupService lookupService) : CrudControllerBase(context)
{
    private static readonly ApplicationSupplementRequestQueryDefinition QueryDefinition = new();

    public async Task<IActionResult> Index(ApplicationSupplementRequestIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.ApplicationSupplementRequests, query, QueryDefinition, cancellationToken);

        return View(new ListPageViewModel<ApplicationSupplementRequestIndexQuery, ApplicationSupplementRequestListItem>
        {
            Query = query,
            Results = results
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationSupplementRequests
            .AsNoTracking()
            .Include(x => x.Application)
            .Include(x => x.RequestedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ApplicationSupplementRequestFormModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationSupplementRequestFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        Context.ApplicationSupplementRequests.Add(new ApplicationSupplementRequest
        {
            Id = Guid.NewGuid(),
            ApplicationId = model.ApplicationId,
            RequestedBy = model.RequestedBy,
            RequestedAt = model.RequestedAt,
            DueAt = model.DueAt,
            RequestContent = model.RequestContent,
            Status = model.Status,
            ResolvedAt = model.ResolvedAt
        });

        if (!await TrySaveChangesAsync("Unable to create application supplement request."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Application supplement request created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationSupplementRequests.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var model = new ApplicationSupplementRequestFormModel
        {
            ApplicationId = entity.ApplicationId,
            RequestedBy = entity.RequestedBy,
            RequestedAt = entity.RequestedAt,
            DueAt = entity.DueAt,
            RequestContent = entity.RequestContent,
            Status = entity.Status,
            ResolvedAt = entity.ResolvedAt
        };

        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ApplicationSupplementRequestFormModel model, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationSupplementRequests.FindAsync([id], cancellationToken);
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
        entity.RequestedBy = model.RequestedBy;
        entity.RequestedAt = model.RequestedAt;
        entity.DueAt = model.DueAt;
        entity.RequestContent = model.RequestContent;
        entity.Status = model.Status;
        entity.ResolvedAt = model.ResolvedAt;

        if (!await TrySaveChangesAsync("Unable to update application supplement request."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Application supplement request updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationSupplementRequests.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await TryDeleteAsync(entity, "Unable to delete application supplement request."))
        {
            TempData["SuccessMessage"] = "Application supplement request deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(ApplicationSupplementRequestFormModel model, CancellationToken cancellationToken)
    {
        model.ApplicationOptions = await lookupService.GetApplicationOptionsAsync(cancellationToken: cancellationToken);
        model.UserOptions = await lookupService.GetUserOptionsAsync(cancellationToken: cancellationToken);
    }
}
