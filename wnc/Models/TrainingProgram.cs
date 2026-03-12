using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class TrainingProgram
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string ProgramCode { get; set; } = string.Empty;
    [MaxLength(255)] public string ProgramName { get; set; } = string.Empty;
    [MaxLength(50)] public string EducationType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TuitionFee { get; set; }
    [MaxLength(100)] public string? DurationText { get; set; }
    public int Quota { get; set; }
    [MaxLength(255)] public string? ManagingUnit { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Major> Majors { get; set; } = new List<Major>();
    public ICollection<RoundProgram> RoundPrograms { get; set; } = new List<RoundProgram>();
    public ICollection<ApplicationPreference> ApplicationPreferences { get; set; } = new List<ApplicationPreference>();
}
