using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class ApplicationSupplementRequest
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public string RequestContent { get; set; } = string.Empty;
    [MaxLength(30)] public string Status { get; set; } = "OPEN";
    public DateTime? ResolvedAt { get; set; }

    public AdmissionApplication Application { get; set; } = null!;
    public AppUser RequestedByUser { get; set; } = null!;
}
