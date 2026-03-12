using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wnc.Data;

namespace wnc.Features.Common.Lookups;

public class CrudLookupService(AppDbContext context)
{
    public async Task<IReadOnlyList<SelectListItem>> GetApplicationOptionsAsync(
        bool includeBlank = false,
        CancellationToken cancellationToken = default)
    {
        var applications = await context.AdmissionApplications
            .AsNoTracking()
            .OrderBy(x => x.ApplicationCode)
            .Select(x => new { x.Id, x.ApplicationCode })
            .ToListAsync(cancellationToken);

        return ToSelectList(
            applications.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.ApplicationCode
            }),
            includeBlank);
    }

    public async Task<IReadOnlyList<SelectListItem>> GetUserOptionsAsync(
        bool includeBlank = false,
        CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(x => x.Username ?? x.Email)
            .Select(x => new { x.Id, x.Username, x.Email })
            .ToListAsync(cancellationToken);

        return ToSelectList(
            users.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(x.Username)
                    ? x.Email ?? x.Id.ToString()
                    : string.IsNullOrWhiteSpace(x.Email)
                        ? x.Username
                        : $"{x.Username} ({x.Email})"
            }),
            includeBlank);
    }

    public async Task<IReadOnlyList<SelectListItem>> GetNotificationTemplateOptionsAsync(
        bool includeBlank = false,
        CancellationToken cancellationToken = default)
    {
        var templates = await context.NotificationTemplates
            .AsNoTracking()
            .OrderBy(x => x.TemplateCode)
            .Select(x => new { x.Id, x.TemplateCode, x.TemplateName })
            .ToListAsync(cancellationToken);

        return ToSelectList(
            templates.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.TemplateCode} - {x.TemplateName}"
            }),
            includeBlank);
    }

    private static IReadOnlyList<SelectListItem> ToSelectList(
        IEnumerable<SelectListItem> items,
        bool includeBlank)
    {
        var result = items.ToList();

        if (includeBlank)
        {
            result.Insert(0, new SelectListItem
            {
                Value = string.Empty,
                Text = "-- Optional --"
            });
        }

        return result;
    }
}
