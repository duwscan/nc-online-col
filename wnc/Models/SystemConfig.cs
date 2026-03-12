using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class SystemConfig
{
    public Guid Id { get; set; }
    [MaxLength(100)] public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AppUser? UpdatedByUser { get; set; }
}
