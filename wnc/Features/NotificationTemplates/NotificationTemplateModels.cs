using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Features.NotificationTemplates;

public class NotificationTemplateIndexQuery : ListQueryParameters
{
    public string? Status { get; set; }
    public string? Channel { get; set; }

    public string? NormalizedStatus => string.IsNullOrWhiteSpace(Status) ? null : Status.Trim();
    public string? NormalizedChannel => string.IsNullOrWhiteSpace(Channel) ? null : Channel.Trim();
}

public class NotificationTemplateListItem
{
    public required Guid Id { get; init; }
    public required string TemplateCode { get; init; }
    public required string TemplateName { get; init; }
    public required string Channel { get; init; }
    public string? SubjectTemplate { get; init; }
    public required string Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class NotificationTemplateFormModel
{
    [Display(Name = "Template code")]
    [Required]
    [StringLength(50)]
    public string TemplateCode { get; set; } = string.Empty;

    [Display(Name = "Template name")]
    [Required]
    [StringLength(255)]
    public string TemplateName { get; set; } = string.Empty;

    [Display(Name = "Channel")]
    [Required]
    [StringLength(30)]
    public string Channel { get; set; } = string.Empty;

    [Display(Name = "Subject template")]
    [StringLength(255)]
    public string? SubjectTemplate { get; set; }

    [Display(Name = "Body template")]
    [Required]
    public string BodyTemplate { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "ACTIVE";

    [Display(Name = "Created at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated at")]
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationTemplateQueryDefinition
    : IQueryDefinition<NotificationTemplate, NotificationTemplateListItem, NotificationTemplateIndexQuery>
{
    public string DefaultSortBy => "updatedat";
    public bool DefaultSortDescending => true;

    public IReadOnlyDictionary<string, LambdaExpression> SortExpressions => new Dictionary<string, LambdaExpression>
    {
        ["templatecode"] = (Expression<Func<NotificationTemplate, string>>)(x => x.TemplateCode),
        ["templatename"] = (Expression<Func<NotificationTemplate, string>>)(x => x.TemplateName),
        ["channel"] = (Expression<Func<NotificationTemplate, string>>)(x => x.Channel),
        ["status"] = (Expression<Func<NotificationTemplate, string>>)(x => x.Status),
        ["updatedat"] = (Expression<Func<NotificationTemplate, DateTime>>)(x => x.UpdatedAt)
    };

    public Expression<Func<NotificationTemplate, NotificationTemplateListItem>> Selector =>
        x => new NotificationTemplateListItem
        {
            Id = x.Id,
            TemplateCode = x.TemplateCode,
            TemplateName = x.TemplateName,
            Channel = x.Channel,
            SubjectTemplate = x.SubjectTemplate,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

    public IQueryable<NotificationTemplate> ApplyIncludes(IQueryable<NotificationTemplate> query)
    {
        return query;
    }

    public IQueryable<NotificationTemplate> ApplyFilters(
        IQueryable<NotificationTemplate> query,
        NotificationTemplateIndexQuery request)
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

    public IQueryable<NotificationTemplate> ApplySearch(
        IQueryable<NotificationTemplate> query,
        NotificationTemplateIndexQuery request)
    {
        if (request.NormalizedSearch is null)
        {
            return query;
        }

        var pattern = $"%{request.NormalizedSearch}%";

        return query.Where(x =>
            EF.Functions.Like(x.TemplateCode, pattern) ||
            EF.Functions.Like(x.TemplateName, pattern) ||
            EF.Functions.Like(x.Channel, pattern) ||
            EF.Functions.Like(x.Status, pattern) ||
            (x.SubjectTemplate != null && EF.Functions.Like(x.SubjectTemplate, pattern)) ||
            EF.Functions.Like(x.BodyTemplate, pattern));
    }
}
