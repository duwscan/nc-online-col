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

    [HttpGet]
    public async Task<IActionResult> ListJson(ApplicationReviewNoteIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.ApplicationReviewNotes, query, QueryDefinition, cancellationToken);
        return Json(results);
    }

    [HttpGet]
    public async Task<IActionResult> GetCreateLookups(CancellationToken cancellationToken)
    {
        var applications = await Context.AdmissionApplications
            .AsNoTracking()
            .OrderBy(a => a.ApplicationCode)
            .Select(a => new { id = a.Id, code = a.ApplicationCode })
            .ToListAsync(cancellationToken);

        var users = await Context.Users
            .AsNoTracking()
            .OrderBy(u => u.Username ?? u.Email)
            .Select(u => new { id = u.Id, display = u.Username ?? u.Email ?? string.Empty })
            .ToListAsync(cancellationToken);

        return Json(new { applications, users });
    }

    [HttpGet]
    public async Task<IActionResult> SearchJson(ApplicationReviewNoteIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.ApplicationReviewNotes, query, QueryDefinition, cancellationToken);
        return Json(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAjax(ApplicationReviewNoteFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kvp => kvp.Value!.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            return Json(new { success = false, errors });
        }

        var entity = new ApplicationReviewNote
        {
            Id = Guid.NewGuid(),
            ApplicationId = model.ApplicationId,
            AuthorUserId = model.AuthorUserId,
            NoteType = model.NoteType,
            Content = model.Content,
            IsVisibleToCandidate = model.IsVisibleToCandidate,
            CreatedAt = model.CreatedAt
        };

        Context.ApplicationReviewNotes.Add(entity);

        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return Json(new { success = false, message = "Unable to create application review note.", detail = ex.Message });
        }

        await Context.Entry(entity).Reference(x => x.Application).LoadAsync(cancellationToken);
        await Context.Entry(entity).Reference(x => x.AuthorUser).LoadAsync(cancellationToken);

        var item = new ApplicationReviewNoteListItem
        {
            Id = entity.Id,
            ApplicationCode = entity.Application.ApplicationCode,
            NoteType = entity.NoteType,
            AuthorDisplay = entity.AuthorUser.Username ?? entity.AuthorUser.Email ?? string.Empty,
            IsVisibleToCandidate = entity.IsVisibleToCandidate,
            CreatedAt = entity.CreatedAt,
            ContentPreview = entity.Content.Length > 120 ? entity.Content.Substring(0, 120) + "..." : entity.Content
        };

        return Json(new { success = true, item });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.ApplicationReviewNotes.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return Json(new { success = false, message = "Record not found." });
        }

        Context.ApplicationReviewNotes.Remove(entity);

        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return Json(new { success = false, message = "Unable to delete application review note.", detail = ex.Message });
        }

        return Json(new { success = true });
    }
}
