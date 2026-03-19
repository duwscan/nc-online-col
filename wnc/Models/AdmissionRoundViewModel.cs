using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AdmissionRoundViewModel
{
    public Guid Id { get; set; }
    public string RoundCode { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public int AdmissionYear { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string? Notes { get; set; }
    public bool AllowEnrollmentConfirmation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedByUserName { get; set; }
    public int ProgramCount { get; set; }
    public int ApplicationCount { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string DateRangeText { get; set; } = string.Empty;
}

public class CreateAdmissionRoundViewModel
{
    [Required(ErrorMessage = "Round Code is required")]
    [MaxLength(50)]
    public string RoundCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Round Name is required")]
    [MaxLength(255)]
    public string RoundName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admission Year is required")]
    [Range(2020, 2100, ErrorMessage = "Invalid year")]
    public int AdmissionYear { get; set; }

    [Required(ErrorMessage = "Start Date is required")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "End Date is required")]
    public DateTime EndAt { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "DRAFT";

    public string? Notes { get; set; }

    public bool AllowEnrollmentConfirmation { get; set; }
}

public class EditAdmissionRoundViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Round Code is required")]
    [MaxLength(50)]
    public string RoundCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Round Name is required")]
    [MaxLength(255)]
    public string RoundName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admission Year is required")]
    [Range(2020, 2100, ErrorMessage = "Invalid year")]
    public int AdmissionYear { get; set; }

    [Required(ErrorMessage = "Start Date is required")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "End Date is required")]
    public DateTime EndAt { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "DRAFT";

    public string? Notes { get; set; }

    public bool AllowEnrollmentConfirmation { get; set; }
}

public class AdmissionRoundSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public int? AdmissionYear { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AdmissionRoundPagedResult
{
    public List<AdmissionRoundViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
