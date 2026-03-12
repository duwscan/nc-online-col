using System.ComponentModel.DataAnnotations;

namespace wnc.ViewModels.TrainingPrograms;

public class TrainingProgramListItemViewModel
{
    public Guid Id { get; set; }
    public string ProgramCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string EducationType { get; set; } = string.Empty;
    public int Quota { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TrainingProgramDetailsViewModel : TrainingProgramFormViewModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TrainingProgramFormViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Program code")]
    public string ProgramCode { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Display(Name = "Program name")]
    public string ProgramName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Education type")]
    public string EducationType { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Tuition fee")]
    [Range(0, 9999999999.99)]
    public decimal? TuitionFee { get; set; }

    [StringLength(100)]
    [Display(Name = "Duration")]
    public string? DurationText { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Quota")]
    public int Quota { get; set; } = 1;

    [StringLength(255)]
    [Display(Name = "Managing unit")]
    public string? ManagingUnit { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";

    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }
}
