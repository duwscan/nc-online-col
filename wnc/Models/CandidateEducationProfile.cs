using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class CandidateEducationProfile
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    [MaxLength(255)] public string SchoolName { get; set; } = string.Empty;
    [MaxLength(50)] public string EducationLevel { get; set; } = string.Empty;
    public int? GraduationYear { get; set; }
    public decimal? Gpa { get; set; }
    [MaxLength(50)] public string? AcademicRank { get; set; }
    [MaxLength(20)] public string? ProvinceCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Candidate Candidate { get; set; } = null!;
}
