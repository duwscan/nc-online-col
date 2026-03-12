using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class Candidate
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(255)] public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    [MaxLength(20)] public string? Gender { get; set; }
    [MaxLength(20)] public string? NationalId { get; set; }
    [MaxLength(255)] public string Email { get; set; } = string.Empty;
    [MaxLength(20)] public string PhoneNumber { get; set; } = string.Empty;
    [MaxLength(255)] public string? AddressLine { get; set; }
    [MaxLength(100)] public string? Ward { get; set; }
    [MaxLength(100)] public string? District { get; set; }
    [MaxLength(20)] public string? ProvinceCode { get; set; }
    public Guid? AvatarFileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public ICollection<CandidateEducationProfile> EducationProfiles { get; set; } = new List<CandidateEducationProfile>();
    public ICollection<CandidatePriorityProfile> PriorityProfiles { get; set; } = new List<CandidatePriorityProfile>();
    public ICollection<AdmissionApplication> AdmissionApplications { get; set; } = new List<AdmissionApplication>();
}
