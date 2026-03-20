using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Common.Lookups;
using wnc.Features.Common.Queries;
using wnc.Features.Notifications;
using wnc.Models;

namespace wnc.Controllers;

public class NotificationsController(
    AppDbContext context,
    QueryPipelineService queryPipeline,
    CrudLookupService lookupService) : CrudControllerBase(context)
{
    private static readonly NotificationQueryDefinition QueryDefinition = new();

    public async Task<IActionResult> Index(NotificationIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.Notifications, query, QueryDefinition, cancellationToken);

        return View(new ListPageViewModel<NotificationIndexQuery, NotificationListItem>
        {
            Query = query,
            Results = results
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.Notifications
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Application)
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new NotificationFormModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotificationFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        Context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = model.UserId,
            ApplicationId = model.ApplicationId,
            TemplateId = model.TemplateId,
            Channel = model.Channel,
            Title = model.Title,
            Content = model.Content,
            Status = model.Status,
            SentAt = model.SentAt,
            ReadAt = model.ReadAt,
            CreatedAt = model.CreatedAt
        });

        if (!await TrySaveChangesAsync("Unable to create notification."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Notification created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.Notifications.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var model = new NotificationFormModel
        {
            UserId = entity.UserId,
            ApplicationId = entity.ApplicationId,
            TemplateId = entity.TemplateId,
            Channel = entity.Channel,
            Title = entity.Title,
            Content = entity.Content,
            Status = entity.Status,
            SentAt = entity.SentAt,
            ReadAt = entity.ReadAt,
            CreatedAt = entity.CreatedAt
        };

        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, NotificationFormModel model, CancellationToken cancellationToken)
    {
        var entity = await Context.Notifications.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        entity.UserId = model.UserId;
        entity.ApplicationId = model.ApplicationId;
        entity.TemplateId = model.TemplateId;
        entity.Channel = model.Channel;
        entity.Title = model.Title;
        entity.Content = model.Content;
        entity.Status = model.Status;
        entity.SentAt = model.SentAt;
        entity.ReadAt = model.ReadAt;
        entity.CreatedAt = model.CreatedAt;

        if (!await TrySaveChangesAsync("Unable to update notification."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Notification updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.Notifications.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await TryDeleteAsync(entity, "Unable to delete notification."))
        {
            TempData["SuccessMessage"] = "Notification deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(NotificationFormModel model, CancellationToken cancellationToken)
    {
        model.UserOptions = await lookupService.GetUserOptionsAsync(cancellationToken: cancellationToken);
        model.ApplicationOptions = await lookupService.GetApplicationOptionsAsync(includeBlank: true, cancellationToken: cancellationToken);
        model.TemplateOptions = await lookupService.GetNotificationTemplateOptionsAsync(includeBlank: true, cancellationToken: cancellationToken);
    }
}
