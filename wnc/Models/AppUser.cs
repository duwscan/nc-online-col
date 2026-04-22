using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AppUser
{
    public Guid Id { get; set; }
    [MaxLength(100)] public string? Username { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? PhoneVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Candidate? Candidate { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserRole> AssignedRoles { get; set; } = new List<UserRole>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<AuthLog> AuthLogs { get; set; } = new List<AuthLog>();
    public ICollection<AdmissionRound> CreatedAdmissionRounds { get; set; } = new List<AdmissionRound>();
    public ICollection<ApplicationStatusHistory> ApplicationStatusHistories { get; set; } = new List<ApplicationStatusHistory>();
    public ICollection<ApplicationReviewNote> ApplicationReviewNotes { get; set; } = new List<ApplicationReviewNote>();
    public ICollection<ApplicationSupplementRequest> ApplicationSupplementRequests { get; set; } = new List<ApplicationSupplementRequest>();
    public ICollection<ApplicationDocument> UploadedDocuments { get; set; } = new List<ApplicationDocument>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<SystemConfig> UpdatedSystemConfigs { get; set; } = new List<SystemConfig>();
}
