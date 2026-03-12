using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    [MaxLength(100)] public string EntityName { get; set; } = string.Empty;
    [MaxLength(100)] public string? EntityId { get; set; }
    [MaxLength(50)] public string Action { get; set; } = string.Empty;
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    [MaxLength(64)] public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser? ActorUser { get; set; }
}
