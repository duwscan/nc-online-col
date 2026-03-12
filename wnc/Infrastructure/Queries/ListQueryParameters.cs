using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace wnc.Infrastructure.Queries;

public abstract class ListQueryParameters
{
    private const int MinPageSize = 5;
    private const int MaxPageSize = 100;

    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(MinPageSize, MaxPageSize)]
    public int PageSize { get; set; } = 10;

    public int GetPage() => Page < 1 ? 1 : Page;

    public int GetPageSize() => Math.Clamp(PageSize, MinPageSize, MaxPageSize);

    public bool IsDescending() => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

    public RouteValueDictionary BuildRouteValues(string? sortBy = null, string? sortDirection = null, int? page = null)
    {
        var values = new RouteValueDictionary();
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? value = property.Name switch
            {
                nameof(SortBy) => sortBy ?? property.GetValue(this),
                nameof(SortDirection) => sortDirection ?? property.GetValue(this),
                nameof(Page) => page ?? property.GetValue(this),
                _ => property.GetValue(this)
            };

            if (value is null)
            {
                continue;
            }

            if (value is string text && string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            values[property.Name] = value switch
            {
                DateTime dateTime => dateTime.ToString("yyyy-MM-dd"),
                bool boolean => boolean,
                _ => value
            };
        }

        return values;
    }
}
