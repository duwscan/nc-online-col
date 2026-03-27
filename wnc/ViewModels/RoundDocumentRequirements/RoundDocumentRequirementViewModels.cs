using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.ViewModels.RoundDocumentRequirements;

public class RoundDocumentRequirementListItemViewModel
{
    public Guid Id { get; set; }
    public string RoundProgramName { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool RequiresNotarization { get; set; }
    public bool RequiresOriginalCopy { get; set; }
    public int MaxFiles { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoundDocumentRequirementDetailsViewModel : RoundDocumentRequirementFormViewModel
{
    public Guid Id { get; set; }
    public string RoundProgramName { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RoundDocumentRequirementFormViewModel
{
    [Required]
    [Display(Name = "Round program")]
    public Guid? RoundProgramId { get; set; }

    [Required]
    [Display(Name = "Document type")]
    public Guid? DocumentTypeId { get; set; }

    [Display(Name = "Is required")]
    public bool IsRequired { get; set; } = true;

    [Display(Name = "Requires notarization")]
    public bool RequiresNotarization { get; set; }

    [Display(Name = "Requires original copy")]
    public bool RequiresOriginalCopy { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Max files")]
    public int MaxFiles { get; set; } = 1;

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> RoundProgramOptions { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> DocumentTypeOptions { get; set; } = Array.Empty<SelectListItem>();
}
