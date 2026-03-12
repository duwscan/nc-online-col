using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Features.ApplicationSupplementRequests;

public class ApplicationSupplementRequestIndexQuery : ListQueryParameters
{
    public string? Status { get; set; }

    public string? NormalizedStatus => string.IsNullOrWhiteSpace(Status) ? null : Status.Trim();
}

public class ApplicationSupplementRequestListItem
{
    public required Guid Id { get; init; }
    public required string ApplicationCode { get; init; }
    public required string RequestedByDisplay { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? DueAt { get; init; }
    public required string Status { get; init; }
    public required string RequestPreview { get; init; }
}

public class ApplicationSupplementRequestFormModel
{
    [Display(Name = "Application")]
    [Required]
    public Guid ApplicationId { get; set; }

    [Display(Name = "Requested by")]
    [Required]
    public Guid RequestedBy { get; set; }

    [Display(Name = "Requested at")]
    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Due at")]
    public DateTime? DueAt { get; set; }

    [Display(Name = "Request content")]
    [Required]
    public string RequestContent { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "OPEN";

    [Display(Name = "Resolved at")]
    public DateTime? ResolvedAt { get; set; }

    public IReadOnlyList<SelectListItem> ApplicationOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> UserOptions { get; set; } = [];
}

public class ApplicationSupplementRequestQueryDefinition
    : IQueryDefinition<ApplicationSupplementRequest, ApplicationSupplementRequestListItem, ApplicationSupplementRequestIndexQuery>
{
    public string DefaultSortBy => "requestat";
    public bool DefaultSortDescending => true;

    public IReadOnlyDictionary<string, LambdaExpression> SortExpressions => new Dictionary<string, LambdaExpression>
    {
        ["application"] = (Expression<Func<ApplicationSupplementRequest, string>>)(x => x.Application.ApplicationCode),
        ["status"] = (Expression<Func<ApplicationSupplementRequest, string>>)(x => x.Status),
        ["requestedat"] = (Expression<Func<ApplicationSupplementRequest, DateTime>>)(x => x.RequestedAt),
        ["dueat"] = (Expression<Func<ApplicationSupplementRequest, DateTime?>>)(x => x.DueAt)
    };

    public Expression<Func<ApplicationSupplementRequest, ApplicationSupplementRequestListItem>> Selector =>
        x => new ApplicationSupplementRequestListItem
        {
            Id = x.Id,
            ApplicationCode = x.Application.ApplicationCode,
            RequestedByDisplay = x.RequestedByUser.Username ?? x.RequestedByUser.Email ?? string.Empty,
            RequestedAt = x.RequestedAt,
            DueAt = x.DueAt,
            Status = x.Status,
            RequestPreview = x.RequestContent.Length > 120 ? x.RequestContent.Substring(0, 120) + "..." : x.RequestContent
        };

    public IQueryable<ApplicationSupplementRequest> ApplyIncludes(IQueryable<ApplicationSupplementRequest> query)
    {
        return query
            .Include(x => x.Application)
            .Include(x => x.RequestedByUser);
    }

    public IQueryable<ApplicationSupplementRequest> ApplyFilters(
        IQueryable<ApplicationSupplementRequest> query,
        ApplicationSupplementRequestIndexQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.NormalizedStatus))
        {
            query = query.Where(x => x.Status == request.NormalizedStatus);
        }

        return query;
    }

    public IQueryable<ApplicationSupplementRequest> ApplySearch(
        IQueryable<ApplicationSupplementRequest> query,
        ApplicationSupplementRequestIndexQuery request)
    {
        if (request.NormalizedSearch is null)
        {
            return query;
        }

        var pattern = $"%{request.NormalizedSearch}%";

        return query.Where(x =>
            EF.Functions.Like(x.Application.ApplicationCode, pattern) ||
            EF.Functions.Like(x.Status, pattern) ||
            EF.Functions.Like(x.RequestContent, pattern) ||
            (x.RequestedByUser.Username != null && EF.Functions.Like(x.RequestedByUser.Username, pattern)) ||
            (x.RequestedByUser.Email != null && EF.Functions.Like(x.RequestedByUser.Email, pattern)));
    }
}
