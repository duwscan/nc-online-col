using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Common.Queries;
using wnc.Features.NotificationTemplates;
using wnc.Models;

namespace wnc.Controllers;

public class NotificationTemplatesController(
    AppDbContext context,
    QueryPipelineService queryPipeline) : CrudControllerBase(context)
{
    private static readonly NotificationTemplateQueryDefinition QueryDefinition = new();

    public async Task<IActionResult> Index(NotificationTemplateIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.NotificationTemplates, query, QueryDefinition, cancellationToken);

        return View(new ListPageViewModel<NotificationTemplateIndexQuery, NotificationTemplateListItem>
        {
            Query = query,
            Results = results
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationTemplates
            .AsNoTracking()
            .Include(x => x.Notifications)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    public IActionResult Create()
    {
        return View("Upsert", new NotificationTemplateFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotificationTemplateFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Upsert", model);
        }

        Context.NotificationTemplates.Add(new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            TemplateCode = model.TemplateCode,
            TemplateName = model.TemplateName,
            Channel = model.Channel,
            SubjectTemplate = model.SubjectTemplate,
            BodyTemplate = model.BodyTemplate,
            Status = model.Status,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        });

        if (!await TrySaveChangesAsync("Unable to create notification template."))
        {
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Notification template created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationTemplates.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        return View("Upsert", new NotificationTemplateFormModel
        {
            TemplateCode = entity.TemplateCode,
            TemplateName = entity.TemplateName,
            Channel = entity.Channel,
            SubjectTemplate = entity.SubjectTemplate,
            BodyTemplate = entity.BodyTemplate,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, NotificationTemplateFormModel model, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationTemplates.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Upsert", model);
        }

        entity.TemplateCode = model.TemplateCode;
        entity.TemplateName = model.TemplateName;
        entity.Channel = model.Channel;
        entity.SubjectTemplate = model.SubjectTemplate;
        entity.BodyTemplate = model.BodyTemplate;
        entity.Status = model.Status;
        entity.CreatedAt = model.CreatedAt;
        entity.UpdatedAt = model.UpdatedAt;

        if (!await TrySaveChangesAsync("Unable to update notification template."))
        {
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Notification template updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationTemplates.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await TryDeleteAsync(entity, "Unable to delete notification template."))
        {
            TempData["SuccessMessage"] = "Notification template deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
