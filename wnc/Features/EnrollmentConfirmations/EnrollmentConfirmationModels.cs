using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Features.EnrollmentConfirmations;

public class EnrollmentConfirmationIndexQuery : ListQueryParameters
{
    public string? ConfirmationStatus { get; set; }

    public string? NormalizedConfirmationStatus => string.IsNullOrWhiteSpace(ConfirmationStatus) ? null : ConfirmationStatus.Trim();
}

public class EnrollmentConfirmationListItem
{
    public required Guid Id { get; init; }
    public required string ApplicationCode { get; init; }
    public required string ConfirmationStatus { get; init; }
    public DateTime? ConfirmedByCandidateAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class EnrollmentConfirmationFormModel
{
    [Display(Name = "Application")]
    [Required]
    public Guid ApplicationId { get; set; }

    [Display(Name = "Confirmed by candidate at")]
    public DateTime? ConfirmedByCandidateAt { get; set; }

    [Display(Name = "Confirmation status")]
    [Required]
    [StringLength(30)]
    public string ConfirmationStatus { get; set; } = "PENDING";

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Created at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated at")]
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public IReadOnlyList<SelectListItem> ApplicationOptions { get; set; } = [];
}

public class EnrollmentConfirmationQueryDefinition
    : IQueryDefinition<EnrollmentConfirmation, EnrollmentConfirmationListItem, EnrollmentConfirmationIndexQuery>
{
    public string DefaultSortBy => "updatedat";
    public bool DefaultSortDescending => true;

    public IReadOnlyDictionary<string, LambdaExpression> SortExpressions => new Dictionary<string, LambdaExpression>
    {
        ["application"] = (Expression<Func<EnrollmentConfirmation, string>>)(x => x.Application.ApplicationCode),
        ["status"] = (Expression<Func<EnrollmentConfirmation, string>>)(x => x.ConfirmationStatus),
        ["confirmedat"] = (Expression<Func<EnrollmentConfirmation, DateTime?>>)(x => x.ConfirmedByCandidateAt),
        ["updatedat"] = (Expression<Func<EnrollmentConfirmation, DateTime>>)(x => x.UpdatedAt)
    };

    public Expression<Func<EnrollmentConfirmation, EnrollmentConfirmationListItem>> Selector =>
        x => new EnrollmentConfirmationListItem
        {
            Id = x.Id,
            ApplicationCode = x.Application.ApplicationCode,
            ConfirmationStatus = x.ConfirmationStatus,
            ConfirmedByCandidateAt = x.ConfirmedByCandidateAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

    public IQueryable<EnrollmentConfirmation> ApplyIncludes(IQueryable<EnrollmentConfirmation> query)
    {
        return query.Include(x => x.Application);
    }

    public IQueryable<EnrollmentConfirmation> ApplyFilters(
        IQueryable<EnrollmentConfirmation> query,
        EnrollmentConfirmationIndexQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.NormalizedConfirmationStatus))
        {
            query = query.Where(x => x.ConfirmationStatus == request.NormalizedConfirmationStatus);
        }

        return query;
    }

    public IQueryable<EnrollmentConfirmation> ApplySearch(
        IQueryable<EnrollmentConfirmation> query,
        EnrollmentConfirmationIndexQuery request)
    {
        if (request.NormalizedSearch is null)
        {
            return query;
        }

        var pattern = $"%{request.NormalizedSearch}%";

        return query.Where(x =>
            EF.Functions.Like(x.Application.ApplicationCode, pattern) ||
            EF.Functions.Like(x.ConfirmationStatus, pattern) ||
            (x.Notes != null && EF.Functions.Like(x.Notes, pattern)));
    }
}
