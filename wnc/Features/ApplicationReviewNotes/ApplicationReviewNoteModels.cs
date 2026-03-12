using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Features.Common.Queries;
using wnc.Models;

namespace wnc.Features.ApplicationReviewNotes;

public class ApplicationReviewNoteIndexQuery : ListQueryParameters
{
    public string? NoteType { get; set; }
    public bool? VisibleToCandidate { get; set; }

    public string? NormalizedNoteType => string.IsNullOrWhiteSpace(NoteType) ? null : NoteType.Trim();
}

public class ApplicationReviewNoteListItem
{
    public required Guid Id { get; init; }
    public required string ApplicationCode { get; init; }
    public required string NoteType { get; init; }
    public required string AuthorDisplay { get; init; }
    public bool IsVisibleToCandidate { get; init; }
    public DateTime CreatedAt { get; init; }
    public required string ContentPreview { get; init; }
}

public class ApplicationReviewNoteFormModel
{
    [Display(Name = "Application")]
    [Required]
    public Guid ApplicationId { get; set; }

    [Display(Name = "Author")]
    [Required]
    public Guid AuthorUserId { get; set; }

    [Display(Name = "Note type")]
    [Required]
    [StringLength(30)]
    public string NoteType { get; set; } = "GENERAL";

    [Display(Name = "Content")]
    [Required]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Visible to candidate")]
    public bool IsVisibleToCandidate { get; set; }

    [Display(Name = "Created at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public IReadOnlyList<SelectListItem> ApplicationOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> UserOptions { get; set; } = [];
}

public class ApplicationReviewNoteQueryDefinition
    : IQueryDefinition<ApplicationReviewNote, ApplicationReviewNoteListItem, ApplicationReviewNoteIndexQuery>
{
    public string DefaultSortBy => "createdat";
    public bool DefaultSortDescending => true;

    public IReadOnlyDictionary<string, LambdaExpression> SortExpressions => new Dictionary<string, LambdaExpression>
    {
        ["application"] = (Expression<Func<ApplicationReviewNote, string>>)(x => x.Application.ApplicationCode),
        ["notetype"] = (Expression<Func<ApplicationReviewNote, string>>)(x => x.NoteType),
        ["visible"] = (Expression<Func<ApplicationReviewNote, bool>>)(x => x.IsVisibleToCandidate),
        ["createdat"] = (Expression<Func<ApplicationReviewNote, DateTime>>)(x => x.CreatedAt)
    };

    public Expression<Func<ApplicationReviewNote, ApplicationReviewNoteListItem>> Selector =>
        x => new ApplicationReviewNoteListItem
        {
            Id = x.Id,
            ApplicationCode = x.Application.ApplicationCode,
            NoteType = x.NoteType,
            AuthorDisplay = x.AuthorUser.Username ?? x.AuthorUser.Email ?? string.Empty,
            IsVisibleToCandidate = x.IsVisibleToCandidate,
            CreatedAt = x.CreatedAt,
            ContentPreview = x.Content.Length > 120 ? x.Content.Substring(0, 120) + "..." : x.Content
        };

    public IQueryable<ApplicationReviewNote> ApplyIncludes(IQueryable<ApplicationReviewNote> query)
    {
        return query
            .Include(x => x.Application)
            .Include(x => x.AuthorUser);
    }

    public IQueryable<ApplicationReviewNote> ApplyFilters(
        IQueryable<ApplicationReviewNote> query,
        ApplicationReviewNoteIndexQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.NormalizedNoteType))
        {
            query = query.Where(x => x.NoteType == request.NormalizedNoteType);
        }

        if (request.VisibleToCandidate.HasValue)
        {
            query = query.Where(x => x.IsVisibleToCandidate == request.VisibleToCandidate.Value);
        }

        return query;
    }

    public IQueryable<ApplicationReviewNote> ApplySearch(
        IQueryable<ApplicationReviewNote> query,
        ApplicationReviewNoteIndexQuery request)
    {
        if (request.NormalizedSearch is null)
        {
            return query;
        }

        var pattern = $"%{request.NormalizedSearch}%";

        return query.Where(x =>
            EF.Functions.Like(x.Application.ApplicationCode, pattern) ||
            EF.Functions.Like(x.NoteType, pattern) ||
            EF.Functions.Like(x.Content, pattern) ||
            (x.AuthorUser.Username != null && EF.Functions.Like(x.AuthorUser.Username, pattern)) ||
            (x.AuthorUser.Email != null && EF.Functions.Like(x.AuthorUser.Email, pattern)));
    }
}
