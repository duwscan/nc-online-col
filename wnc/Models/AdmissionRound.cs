using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AdmissionRound
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string RoundCode { get; set; } = string.Empty;
    [MaxLength(255)] public string RoundName { get; set; } = string.Empty;
    public int AdmissionYear { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "DRAFT";
    public string? Notes { get; set; }
    public bool AllowEnrollmentConfirmation { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public AppUser? CreatedByUser { get; set; }
    public ICollection<RoundProgram> RoundPrograms { get; set; } = new List<RoundProgram>();
}
