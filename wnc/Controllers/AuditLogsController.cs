using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.AuditLogs;
using wnc.Features.Common.Lookups;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Controllers;

public class AuditLogsController(
    AppDbContext context,
    QueryPipelineService queryPipeline,
    CrudLookupService lookupService) : CrudControllerBase(context)
{
    private static readonly AuditLogQueryDefinition QueryDefinition = new();

    public async Task<IActionResult> Index(AuditLogIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.AuditLogs, query, QueryDefinition, cancellationToken);

        return View(new ListPageViewModel<AuditLogIndexQuery, AuditLogListItem>
        {
            Query = query,
            Results = results
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.AuditLogs
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new AuditLogFormModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AuditLogFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        Context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = model.ActorUserId,
            EntityName = model.EntityName,
            EntityId = model.EntityId,
            Action = model.Action,
            OldData = model.OldData,
            NewData = model.NewData,
            IpAddress = model.IpAddress,
            CreatedAt = model.CreatedAt
        });

        if (!await TrySaveChangesAsync("Unable to create audit log."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Audit log created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.AuditLogs.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var model = new AuditLogFormModel
        {
            ActorUserId = entity.ActorUserId,
            EntityName = entity.EntityName,
            EntityId = entity.EntityId,
            Action = entity.Action,
            OldData = entity.OldData,
            NewData = entity.NewData,
            IpAddress = entity.IpAddress,
            CreatedAt = entity.CreatedAt
        };

        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AuditLogFormModel model, CancellationToken cancellationToken)
    {
        var entity = await Context.AuditLogs.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        entity.ActorUserId = model.ActorUserId;
        entity.EntityName = model.EntityName;
        entity.EntityId = model.EntityId;
        entity.Action = model.Action;
        entity.OldData = model.OldData;
        entity.NewData = model.NewData;
        entity.IpAddress = model.IpAddress;
        entity.CreatedAt = model.CreatedAt;

        if (!await TrySaveChangesAsync("Unable to update audit log."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Audit log updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.AuditLogs.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await TryDeleteAsync(entity, "Unable to delete audit log."))
        {
            TempData["SuccessMessage"] = "Audit log deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(AuditLogFormModel model, CancellationToken cancellationToken)
    {
        model.UserOptions = await lookupService.GetUserOptionsAsync(includeBlank: true, cancellationToken: cancellationToken);
    }
}
