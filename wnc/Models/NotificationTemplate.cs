using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class NotificationTemplate
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string TemplateCode { get; set; } = string.Empty;
    [MaxLength(255)] public string TemplateName { get; set; } = string.Empty;
    [MaxLength(30)] public string Channel { get; set; } = string.Empty;
    [MaxLength(255)] public string? SubjectTemplate { get; set; }
    public string BodyTemplate { get; set; } = string.Empty;
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
