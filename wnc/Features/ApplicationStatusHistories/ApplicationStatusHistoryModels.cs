using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Features.ApplicationStatusHistories;

public class ApplicationStatusHistoryIndexQuery : ListQueryParameters
{
    public string? ToStatus { get; set; }

    public string? NormalizedToStatus => string.IsNullOrWhiteSpace(ToStatus) ? null : ToStatus.Trim();
}

public class ApplicationStatusHistoryListItem
{
    public required Guid Id { get; init; }
    public required string ApplicationCode { get; init; }
    public string? FromStatus { get; init; }
    public required string ToStatus { get; init; }
    public string ChangedByDisplay { get; init; } = string.Empty;
    public DateTime ChangedAt { get; init; }
    public string? Reason { get; init; }
}

public class ApplicationStatusHistoryFormModel
{
    [Display(Name = "Application")]
    [Required]
    public Guid ApplicationId { get; set; }

    [Display(Name = "Previous status")]
    [StringLength(30)]
    public string? FromStatus { get; set; }

    [Display(Name = "New status")]
    [Required]
    [StringLength(30)]
    public string ToStatus { get; set; } = string.Empty;

    [Display(Name = "Changed by")]
    public Guid? ChangedBy { get; set; }

    [Display(Name = "Changed at")]
    [Required]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Reason")]
    public string? Reason { get; set; }

    [Display(Name = "Public note")]
    public string? PublicNote { get; set; }

    [Display(Name = "Internal note")]
    public string? InternalNote { get; set; }

    public IReadOnlyList<SelectListItem> ApplicationOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> UserOptions { get; set; } = [];
}

public class ApplicationStatusHistoryQueryDefinition
    : IQueryDefinition<ApplicationStatusHistory, ApplicationStatusHistoryListItem, ApplicationStatusHistoryIndexQuery>
{
    public string DefaultSortBy => "changedat";
    public bool DefaultSortDescending => true;

    public IReadOnlyDictionary<string, LambdaExpression> SortExpressions => new Dictionary<string, LambdaExpression>
    {
        ["application"] = (Expression<Func<ApplicationStatusHistory, string>>)(x => x.Application.ApplicationCode),
        ["fromstatus"] = (Expression<Func<ApplicationStatusHistory, string?>>)(x => x.FromStatus),
        ["tostatus"] = (Expression<Func<ApplicationStatusHistory, string>>)(x => x.ToStatus),
        ["changedat"] = (Expression<Func<ApplicationStatusHistory, DateTime>>)(x => x.ChangedAt)
    };

    public Expression<Func<ApplicationStatusHistory, ApplicationStatusHistoryListItem>> Selector =>
        x => new ApplicationStatusHistoryListItem
        {
            Id = x.Id,
            ApplicationCode = x.Application.ApplicationCode,
            FromStatus = x.FromStatus,
            ToStatus = x.ToStatus,
            ChangedByDisplay = x.ChangedByUser == null
                ? "(system)"
                : x.ChangedByUser.Username ?? x.ChangedByUser.Email ?? string.Empty,
            ChangedAt = x.ChangedAt,
            Reason = x.Reason
        };

    public IQueryable<ApplicationStatusHistory> ApplyIncludes(IQueryable<ApplicationStatusHistory> query)
    {
        return query
            .Include(x => x.Application)
            .Include(x => x.ChangedByUser);
    }

    public IQueryable<ApplicationStatusHistory> ApplyFilters(
        IQueryable<ApplicationStatusHistory> query,
        ApplicationStatusHistoryIndexQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.NormalizedToStatus))
        {
            query = query.Where(x => x.ToStatus == request.NormalizedToStatus);
        }

        return query;
    }

    public IQueryable<ApplicationStatusHistory> ApplySearch(
        IQueryable<ApplicationStatusHistory> query,
        ApplicationStatusHistoryIndexQuery request)
    {
        if (request.NormalizedSearch is null)
        {
            return query;
        }

        var pattern = $"%{request.NormalizedSearch}%";

        return query.Where(x =>
            EF.Functions.Like(x.Application.ApplicationCode, pattern) ||
            (x.FromStatus != null && EF.Functions.Like(x.FromStatus, pattern)) ||
            EF.Functions.Like(x.ToStatus, pattern) ||
            (x.Reason != null && EF.Functions.Like(x.Reason, pattern)) ||
            (x.PublicNote != null && EF.Functions.Like(x.PublicNote, pattern)) ||
            (x.InternalNote != null && EF.Functions.Like(x.InternalNote, pattern)) ||
            (x.ChangedByUser != null && x.ChangedByUser.Username != null && EF.Functions.Like(x.ChangedByUser.Username, pattern)) ||
            (x.ChangedByUser != null && x.ChangedByUser.Email != null && EF.Functions.Like(x.ChangedByUser.Email, pattern)));
    }
}
