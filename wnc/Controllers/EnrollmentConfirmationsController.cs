using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Common.Lookups;
using wnc.Features.Common.Queries;
using wnc.Features.EnrollmentConfirmations;
using wnc.Models;

namespace wnc.Controllers;

public class EnrollmentConfirmationsController(
    AppDbContext context,
    QueryPipelineService queryPipeline,
    CrudLookupService lookupService) : CrudControllerBase(context)
{
    private static readonly EnrollmentConfirmationQueryDefinition QueryDefinition = new();

    public async Task<IActionResult> Index(EnrollmentConfirmationIndexQuery query, CancellationToken cancellationToken)
    {
        var results = await queryPipeline.ExecuteAsync(Context.EnrollmentConfirmations, query, QueryDefinition, cancellationToken);

        return View(new ListPageViewModel<EnrollmentConfirmationIndexQuery, EnrollmentConfirmationListItem>
        {
            Query = query,
            Results = results
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.EnrollmentConfirmations
            .AsNoTracking()
            .Include(x => x.Application)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? NotFound() : View(entity);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new EnrollmentConfirmationFormModel();
        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EnrollmentConfirmationFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        Context.EnrollmentConfirmations.Add(new EnrollmentConfirmation
        {
            Id = Guid.NewGuid(),
            ApplicationId = model.ApplicationId,
            ConfirmedByCandidateAt = model.ConfirmedByCandidateAt,
            ConfirmationStatus = model.ConfirmationStatus,
            Notes = model.Notes,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        });

        if (!await TrySaveChangesAsync("Unable to create enrollment confirmation."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Enrollment confirmation created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.EnrollmentConfirmations.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        var model = new EnrollmentConfirmationFormModel
        {
            ApplicationId = entity.ApplicationId,
            ConfirmedByCandidateAt = entity.ConfirmedByCandidateAt,
            ConfirmationStatus = entity.ConfirmationStatus,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

        await PopulateLookupsAsync(model, cancellationToken);
        return View("Upsert", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EnrollmentConfirmationFormModel model, CancellationToken cancellationToken)
    {
        var entity = await Context.EnrollmentConfirmations.FindAsync([id], cancellationToken);
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
        entity.ConfirmedByCandidateAt = model.ConfirmedByCandidateAt;
        entity.ConfirmationStatus = model.ConfirmationStatus;
        entity.Notes = model.Notes;
        entity.CreatedAt = model.CreatedAt;
        entity.UpdatedAt = model.UpdatedAt;

        if (!await TrySaveChangesAsync("Unable to update enrollment confirmation."))
        {
            await PopulateLookupsAsync(model, cancellationToken);
            return View("Upsert", model);
        }

        TempData["SuccessMessage"] = "Enrollment confirmation updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.EnrollmentConfirmations.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (await TryDeleteAsync(entity, "Unable to delete enrollment confirmation."))
        {
            TempData["SuccessMessage"] = "Enrollment confirmation deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(EnrollmentConfirmationFormModel model, CancellationToken cancellationToken)
    {
        model.ApplicationOptions = await lookupService.GetApplicationOptionsAsync(cancellationToken: cancellationToken);
    }
}
