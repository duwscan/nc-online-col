using System.ComponentModel.DataAnnotations;

namespace wnc.ViewModels.AdmissionMethods;

public class AdmissionMethodListItemViewModel
{
    public Guid Id { get; set; }
    public string MethodCode { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class AdmissionMethodDetailsViewModel : AdmissionMethodFormViewModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdmissionMethodFormViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Method code")]
    public string MethodCode { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Display(Name = "Method name")]
    public string MethodName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";
}
