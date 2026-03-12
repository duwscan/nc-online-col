using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class RoundAdmissionMethod
{
    public Guid Id { get; set; }
    public Guid RoundProgramId { get; set; }
    public Guid MethodId { get; set; }
    [MaxLength(50)] public string? CombinationCode { get; set; }
    public decimal? MinimumScore { get; set; }
    public string? PriorityPolicy { get; set; }
    public string? CalculationRule { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }

    public RoundProgram RoundProgram { get; set; } = null!;
    public AdmissionMethod Method { get; set; } = null!;
}
