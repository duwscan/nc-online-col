using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.ViewModels.RoundAdmissionMethods;

public class RoundAdmissionMethodListItemViewModel
{
    public Guid Id { get; set; }
    public string RoundProgramName { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public string? CombinationCode { get; set; }
    public decimal? MinimumScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RoundAdmissionMethodDetailsViewModel : RoundAdmissionMethodFormViewModel
{
    public Guid Id { get; set; }
    public string RoundProgramName { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RoundAdmissionMethodFormViewModel
{
    [Required]
    [Display(Name = "Round program")]
    public Guid? RoundProgramId { get; set; }

    [Required]
    [Display(Name = "Admission method")]
    public Guid? MethodId { get; set; }

    [StringLength(50)]
    [Display(Name = "Combination code")]
    public string? CombinationCode { get; set; }

    [Range(0, 100)]
    [Display(Name = "Minimum score")]
    public decimal? MinimumScore { get; set; }

    [Display(Name = "Priority policy")]
    public string? PriorityPolicy { get; set; }

    [Display(Name = "Calculation rule")]
    public string? CalculationRule { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";

    public IEnumerable<SelectListItem> RoundProgramOptions { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> MethodOptions { get; set; } = Array.Empty<SelectListItem>();
}
