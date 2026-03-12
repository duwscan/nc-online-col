using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ApplicationId { get; set; }
    public Guid? TemplateId { get; set; }
    [MaxLength(30)] public string Channel { get; set; } = "IN_APP";
    [MaxLength(255)] public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    [MaxLength(30)] public string Status { get; set; } = "PENDING";
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public AdmissionApplication? Application { get; set; }
    public NotificationTemplate? Template { get; set; }
}
