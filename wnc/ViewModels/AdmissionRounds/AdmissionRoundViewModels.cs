using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.ViewModels.AdmissionRounds;

public class AdmissionRoundListItemViewModel
{
    public Guid Id { get; set; }
    public string RoundCode { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public int AdmissionYear { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AllowEnrollmentConfirmation { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class AdmissionRoundDetailsViewModel : AdmissionRoundFormViewModel
{
    public Guid Id { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdmissionRoundFormViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Round code")]
    public string RoundCode { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Display(Name = "Round name")]
    public string RoundName { get; set; } = string.Empty;

    [Range(2000, 2100)]
    [Display(Name = "Admission year")]
    public int AdmissionYear { get; set; } = DateTime.UtcNow.Year;

    [DataType(DataType.DateTime)]
    [Display(Name = "Start at")]
    public DateTime StartAt { get; set; } = DateTime.UtcNow;

    [DataType(DataType.DateTime)]
    [Display(Name = "End at")]
    public DateTime EndAt { get; set; } = DateTime.UtcNow.AddDays(30);

    [Required, StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "DRAFT";

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Allow enrollment confirmation")]
    public bool AllowEnrollmentConfirmation { get; set; }

    [Display(Name = "Created by")]
    public Guid? CreatedBy { get; set; }

    public IEnumerable<SelectListItem> CreatedByOptions { get; set; } = Array.Empty<SelectListItem>();
}
