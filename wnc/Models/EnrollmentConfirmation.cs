using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class EnrollmentConfirmation
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public DateTime? ConfirmedByCandidateAt { get; set; }
    [MaxLength(30)] public string ConfirmationStatus { get; set; } = "PENDING";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AdmissionApplication Application { get; set; } = null!;
}
