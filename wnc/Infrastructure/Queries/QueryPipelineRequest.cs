using System.ComponentModel.DataAnnotations;

namespace wnc.Infrastructure.Queries;

public class QueryPipelineRequest
{
    [Display(Name = "Từ khóa")]
    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => PageSize
    };

    public int SkipCount => (NormalizedPage - 1) * NormalizedPageSize;
}
