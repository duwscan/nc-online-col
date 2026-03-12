using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class CandidatePriorityProfile
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    [MaxLength(30)] public string PriorityType { get; set; } = string.Empty;
    [MaxLength(50)] public string PriorityCode { get; set; } = string.Empty;
    public decimal? ScoreValue { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Candidate Candidate { get; set; } = null!;
}
