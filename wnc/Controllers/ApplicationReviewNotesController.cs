using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.ApplicationReviewNotes;
using wnc.Features.Common.Lookups;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Controllers;

public class ApplicationReviewNotesController(
    AppDbContext context,
    QueryPipelineService queryPipeline,
    CrudLookupService lookupService) : CrudControllerBase(context)
{
    private static readonly ApplicationReviewNoteQueryDefinition QueryDefinition = new();

    public async Task<IActionResult> Index(ApplicationReviewNoteIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.ApplicationReviewNotes, query, QueryDefinition, cancellationToken);

        return View(new ListPageViewModel<ApplicationReviewNoteIndexQuery, ApplicationReviewNoteListItem>
        {
            Query = query,
            Results = results
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationReviewNotes
            .AsNoTracking()
            .Include(x => x.Application)
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ApplicationReviewNoteFormModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationReviewNoteFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        Context.ApplicationReviewNotes.Add(new ApplicationReviewNote
        {
            Id = Guid.NewGuid(),
            ApplicationId = model.ApplicationId,
            AuthorUserId = model.AuthorUserId,
            NoteType = model.NoteType,
            Content = model.Content,
            IsVisibleToCandidate = model.IsVisibleToCandidate,
            CreatedAt = model.CreatedAt
        });

        if (!await TrySaveChangesAsync("Unable to create application review note."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Application review note created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationReviewNotes.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var model = new ApplicationReviewNoteFormModel
        {
            ApplicationId = entity.ApplicationId,
            AuthorUserId = entity.AuthorUserId,
            NoteType = entity.NoteType,
            Content = entity.Content,
            IsVisibleToCandidate = entity.IsVisibleToCandidate,
            CreatedAt = entity.CreatedAt
        };

        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ApplicationReviewNoteFormModel model, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationReviewNotes.FindAsync([id], cancellationToken);
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
        entity.AuthorUserId = model.AuthorUserId;
        entity.NoteType = model.NoteType;
        entity.Content = model.Content;
        entity.IsVisibleToCandidate = model.IsVisibleToCandidate;
        entity.CreatedAt = model.CreatedAt;

        if (!await TrySaveChangesAsync("Unable to update application review note."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Application review note updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationReviewNotes.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await TryDeleteAsync(entity, "Unable to delete application review note."))
        {
            TempData["SuccessMessage"] = "Application review note deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(ApplicationReviewNoteFormModel model, CancellationToken cancellationToken)
    {
        model.ApplicationOptions = await lookupService.GetApplicationOptionsAsync(cancellationToken: cancellationToken);
        model.UserOptions = await lookupService.GetUserOptionsAsync(cancellationToken: cancellationToken);
    }
}
