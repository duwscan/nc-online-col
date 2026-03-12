using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.ViewModels.RoundPrograms;

public class RoundProgramListItemViewModel
{
    public Guid Id { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int Quota { get; set; }
    public int? PublishedQuota { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class RoundProgramDetailsViewModel : RoundProgramFormViewModel
{
    public Guid Id { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RoundProgramFormViewModel
{
    [Required]
    [Display(Name = "Admission round")]
    public Guid? RoundId { get; set; }

    [Required]
    [Display(Name = "Training program")]
    public Guid? ProgramId { get; set; }

    [Display(Name = "Major")]
    public Guid? MajorId { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Quota")]
    public int Quota { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Published quota")]
    public int? PublishedQuota { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";

    public IEnumerable<SelectListItem> RoundOptions { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> ProgramOptions { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> MajorOptions { get; set; } = Array.Empty<SelectListItem>();
}
