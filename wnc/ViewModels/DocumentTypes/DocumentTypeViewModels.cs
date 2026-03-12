using System.ComponentModel.DataAnnotations;

namespace wnc.ViewModels.DocumentTypes;

public class DocumentTypeListItemViewModel
{
    public Guid Id { get; set; }
    public string DocumentCode { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DocumentTypeDetailsViewModel : DocumentTypeFormViewModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DocumentTypeFormViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Document code")]
    public string DocumentCode { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Display(Name = "Document name")]
    public string DocumentName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";
}
