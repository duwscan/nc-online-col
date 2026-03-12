using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.ViewModels.Majors;

public class MajorListItemViewModel
{
    public Guid Id { get; set; }
    public string MajorCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public int Quota { get; set; }
    public int DisplayOrder { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class MajorDetailsViewModel : MajorFormViewModel
{
    public Guid Id { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MajorFormViewModel
{
    [Required]
    [Display(Name = "Training program")]
    public Guid? ProgramId { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Major code")]
    public string MajorCode { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Display(Name = "Major name")]
    public string MajorName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Quota")]
    public int Quota { get; set; }

    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";

    public IEnumerable<SelectListItem> ProgramOptions { get; set; } = Array.Empty<SelectListItem>();
}
