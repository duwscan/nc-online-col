using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AuthLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    [MaxLength(255)] public string LoginIdentifier { get; set; } = string.Empty;
    [MaxLength(20)] public string Status { get; set; } = string.Empty;
    [MaxLength(255)] public string? FailureReason { get; set; }
    [MaxLength(64)] public string? IpAddress { get; set; }
    [MaxLength(500)] public string? UserAgent { get; set; }
    public DateTime LoggedAt { get; set; }

    public AppUser? User { get; set; }
}
