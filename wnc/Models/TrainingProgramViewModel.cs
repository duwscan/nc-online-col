using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class TrainingProgramViewModel
{
    public Guid Id { get; set; }
    public string ProgramCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string EducationType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TuitionFee { get; set; }
    public string? DurationText { get; set; }
    public int Quota { get; set; }
    public string? ManagingUnit { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MajorCount { get; set; }
}

public class CreateTrainingProgramViewModel
{
    [Required(ErrorMessage = "Program Code is required")]
    [MaxLength(50)]
    public string ProgramCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Program Name is required")]
    [MaxLength(255)]
    public string ProgramName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Education Type is required")]
    [MaxLength(50)]
    public string EducationType { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Tuition Fee must be positive")]
    public decimal? TuitionFee { get; set; }

    [MaxLength(100)]
    public string? DurationText { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quota must be at least 1")]
    public int Quota { get; set; }

    [MaxLength(255)]
    public string? ManagingUnit { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "ACTIVE";

    public int DisplayOrder { get; set; }
}

public class EditTrainingProgramViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Program Code is required")]
    [MaxLength(50)]
    public string ProgramCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Program Name is required")]
    [MaxLength(255)]
    public string ProgramName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Education Type is required")]
    [MaxLength(50)]
    public string EducationType { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Tuition Fee must be positive")]
    public decimal? TuitionFee { get; set; }

    [MaxLength(100)]
    public string? DurationText { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quota must be at least 1")]
    public int Quota { get; set; }

    [MaxLength(255)]
    public string? ManagingUnit { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "ACTIVE";

    public int DisplayOrder { get; set; }
}

public class TrainingProgramSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? EducationType { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class TrainingProgramPagedResult
{
    public List<TrainingProgramViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
