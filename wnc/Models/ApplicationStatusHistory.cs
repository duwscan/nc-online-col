using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class ApplicationStatusHistory
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    [MaxLength(30)] public string? FromStatus { get; set; }
    [MaxLength(30)] public string ToStatus { get; set; } = string.Empty;
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
    public string? PublicNote { get; set; }
    public string? InternalNote { get; set; }

    public AdmissionApplication Application { get; set; } = null!;
    public AppUser? ChangedByUser { get; set; }
}
