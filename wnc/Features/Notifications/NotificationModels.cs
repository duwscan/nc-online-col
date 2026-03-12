using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Features.Notifications;

public class NotificationIndexQuery : ListQueryParameters
{
    public string? Status { get; set; }
    public string? Channel { get; set; }

    public string? NormalizedStatus => string.IsNullOrWhiteSpace(Status) ? null : Status.Trim();
    public string? NormalizedChannel => string.IsNullOrWhiteSpace(Channel) ? null : Channel.Trim();
}

public class NotificationListItem
{
    public required Guid Id { get; init; }
    public required string UserDisplay { get; init; }
    public string? ApplicationCode { get; init; }
    public string? TemplateCode { get; init; }
    public required string Channel { get; init; }
    public required string Title { get; init; }
    public required string Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
}

public class NotificationFormModel
{
    [Display(Name = "User")]
    [Required]
    public Guid UserId { get; set; }

    [Display(Name = "Application")]
    public Guid? ApplicationId { get; set; }

    [Display(Name = "Template")]
    public Guid? TemplateId { get; set; }

    [Display(Name = "Channel")]
    [Required]
    [StringLength(30)]
    public string Channel { get; set; } = "IN_APP";

    [Display(Name = "Title")]
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Content")]
    [Required]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "PENDING";

    [Display(Name = "Sent at")]
    public DateTime? SentAt { get; set; }

    [Display(Name = "Read at")]
    public DateTime? ReadAt { get; set; }

    [Display(Name = "Created at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public IReadOnlyList<SelectListItem> UserOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> ApplicationOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> TemplateOptions { get; set; } = [];
}

public class NotificationQueryDefinition : IQueryDefinition<Notification, NotificationListItem, NotificationIndexQuery>
{
    public string DefaultSortBy => "createdat";
    public bool DefaultSortDescending => true;

    public IReadOnlyDictionary<string, LambdaExpression> SortExpressions => new Dictionary<string, LambdaExpression>
    {
        ["user"] = (Expression<Func<Notification, string>>)(x => x.User.Username ?? x.User.Email ?? string.Empty),
        ["application"] = (Expression<Func<Notification, string?>>)(x => x.Application != null ? x.Application.ApplicationCode : null),
        ["channel"] = (Expression<Func<Notification, string>>)(x => x.Channel),
        ["status"] = (Expression<Func<Notification, string>>)(x => x.Status),
        ["createdat"] = (Expression<Func<Notification, DateTime>>)(x => x.CreatedAt),
        ["sentat"] = (Expression<Func<Notification, DateTime?>>)(x => x.SentAt)
    };

    public Expression<Func<Notification, NotificationListItem>> Selector =>
        x => new NotificationListItem
        {
            Id = x.Id,
            UserDisplay = x.User.Username ?? x.User.Email ?? string.Empty,
            ApplicationCode = x.Application != null ? x.Application.ApplicationCode : null,
            TemplateCode = x.Template != null ? x.Template.TemplateCode : null,
            Channel = x.Channel,
            Title = x.Title,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            SentAt = x.SentAt
        };

    public IQueryable<Notification> ApplyIncludes(IQueryable<Notification> query)
    {
        return query
            .Include(x => x.User)
            .Include(x => x.Application)
            .Include(x => x.Template);
    }

    public IQueryable<Notification> ApplyFilters(IQueryable<Notification> query, NotificationIndexQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.NormalizedStatus))
        {
            query = query.Where(x => x.Status == request.NormalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(request.NormalizedChannel))
        {
            query = query.Where(x => x.Channel == request.NormalizedChannel);
        }

        return query;
    }

    public IQueryable<Notification> ApplySearch(IQueryable<Notification> query, NotificationIndexQuery request)
    {
        if (request.NormalizedSearch is null)
        {
            return query;
        }

        var pattern = $"%{request.NormalizedSearch}%";

        return query.Where(x =>
            EF.Functions.Like(x.Title, pattern) ||
            EF.Functions.Like(x.Content, pattern) ||
            EF.Functions.Like(x.Channel, pattern) ||
            EF.Functions.Like(x.Status, pattern) ||
            (x.Application != null && EF.Functions.Like(x.Application.ApplicationCode, pattern)) ||
            (x.Template != null && EF.Functions.Like(x.Template.TemplateCode, pattern)) ||
            (x.User.Username != null && EF.Functions.Like(x.User.Username, pattern)) ||
            (x.User.Email != null && EF.Functions.Like(x.User.Email, pattern)));
    }
}
