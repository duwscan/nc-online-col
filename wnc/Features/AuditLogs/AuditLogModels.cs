using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Features.AuditLogs;

public class AuditLogIndexQuery : ListQueryParameters
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }

    public string? NormalizedEntityName => string.IsNullOrWhiteSpace(EntityName) ? null : EntityName.Trim();
    public string? NormalizedAction => string.IsNullOrWhiteSpace(Action) ? null : Action.Trim();
}

public class AuditLogListItem
{
    public required Guid Id { get; init; }
    public string ActorDisplay { get; init; } = string.Empty;
    public required string EntityName { get; init; }
    public string? EntityId { get; init; }
    public required string Action { get; init; }
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class AuditLogFormModel
{
    [Display(Name = "Actor user")]
    public Guid? ActorUserId { get; set; }

    [Display(Name = "Entity name")]
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [Display(Name = "Entity id")]
    [StringLength(100)]
    public string? EntityId { get; set; }

    [Display(Name = "Action")]
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;

    [Display(Name = "Old data")]
    public string? OldData { get; set; }

    [Display(Name = "New data")]
    public string? NewData { get; set; }

    [Display(Name = "IP address")]
    [StringLength(64)]
    public string? IpAddress { get; set; }

    [Display(Name = "Created at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public IReadOnlyList<SelectListItem> UserOptions { get; set; } = [];
}

public class AuditLogQueryDefinition : IQueryDefinition<AuditLog, AuditLogListItem, AuditLogIndexQuery>
{
    public string DefaultSortBy => "createdat";
    public bool DefaultSortDescending => true;

    public IReadOnlyDictionary<string, LambdaExpression> SortExpressions => new Dictionary<string, LambdaExpression>
    {
        ["actor"] = (Expression<Func<AuditLog, string>>)(x => x.ActorUser != null ? x.ActorUser.Username ?? x.ActorUser.Email ?? string.Empty : string.Empty),
        ["entityname"] = (Expression<Func<AuditLog, string>>)(x => x.EntityName),
        ["action"] = (Expression<Func<AuditLog, string>>)(x => x.Action),
        ["createdat"] = (Expression<Func<AuditLog, DateTime>>)(x => x.CreatedAt)
    };

    public Expression<Func<AuditLog, AuditLogListItem>> Selector =>
        x => new AuditLogListItem
        {
            Id = x.Id,
            ActorDisplay = x.ActorUser == null ? "(system)" : x.ActorUser.Username ?? x.ActorUser.Email ?? string.Empty,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            Action = x.Action,
            IpAddress = x.IpAddress,
            CreatedAt = x.CreatedAt
        };

    public IQueryable<AuditLog> ApplyIncludes(IQueryable<AuditLog> query)
    {
        return query.Include(x => x.ActorUser);
    }

    public IQueryable<AuditLog> ApplyFilters(IQueryable<AuditLog> query, AuditLogIndexQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.NormalizedEntityName))
        {
            query = query.Where(x => x.EntityName == request.NormalizedEntityName);
        }

        if (!string.IsNullOrWhiteSpace(request.NormalizedAction))
        {
            query = query.Where(x => x.Action == request.NormalizedAction);
        }

        return query;
    }

    public IQueryable<AuditLog> ApplySearch(IQueryable<AuditLog> query, AuditLogIndexQuery request)
    {
        if (request.NormalizedSearch is null)
        {
            return query;
        }

        var pattern = $"%{request.NormalizedSearch}%";

        return query.Where(x =>
            EF.Functions.Like(x.EntityName, pattern) ||
            EF.Functions.Like(x.Action, pattern) ||
            (x.EntityId != null && EF.Functions.Like(x.EntityId, pattern)) ||
            (x.IpAddress != null && EF.Functions.Like(x.IpAddress, pattern)) ||
            (x.ActorUser != null && x.ActorUser.Username != null && EF.Functions.Like(x.ActorUser.Username, pattern)) ||
            (x.ActorUser != null && x.ActorUser.Email != null && EF.Functions.Like(x.ActorUser.Email, pattern)));
    }
}
