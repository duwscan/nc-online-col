using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class Major
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    [MaxLength(50)] public string MajorCode { get; set; } = string.Empty;
    [MaxLength(255)] public string MajorName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quota { get; set; }
    public int DisplayOrder { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TrainingProgram Program { get; set; } = null!;
    public ICollection<RoundProgram> RoundPrograms { get; set; } = new List<RoundProgram>();
    public ICollection<ApplicationPreference> ApplicationPreferences { get; set; } = new List<ApplicationPreference>();
}
