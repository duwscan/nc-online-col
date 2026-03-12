using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AdmissionMethod
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string MethodCode { get; set; } = string.Empty;
    [MaxLength(255)] public string MethodName { get; set; } = string.Empty;
    public string? Description { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<RoundAdmissionMethod> RoundAdmissionMethods { get; set; } = new List<RoundAdmissionMethod>();
    public ICollection<ApplicationPreference> ApplicationPreferences { get; set; } = new List<ApplicationPreference>();
}
