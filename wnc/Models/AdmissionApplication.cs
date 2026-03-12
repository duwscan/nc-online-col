using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AdmissionApplication
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string ApplicationCode { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    public Guid RoundProgramId { get; set; }
    [MaxLength(30)] public string CurrentStatus { get; set; } = "DRAFT";
    public int SubmissionNumber { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? LastResubmittedAt { get; set; }
    public DateTime? ReviewDeadlineAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Candidate Candidate { get; set; } = null!;
    public RoundProgram RoundProgram { get; set; } = null!;
    public ICollection<ApplicationPreference> ApplicationPreferences { get; set; } = new List<ApplicationPreference>();
    public ICollection<ApplicationDocument> ApplicationDocuments { get; set; } = new List<ApplicationDocument>();
    public ICollection<ApplicationStatusHistory> ApplicationStatusHistories { get; set; } = new List<ApplicationStatusHistory>();
    public ICollection<ApplicationReviewNote> ApplicationReviewNotes { get; set; } = new List<ApplicationReviewNote>();
    public ICollection<ApplicationSupplementRequest> ApplicationSupplementRequests { get; set; } = new List<ApplicationSupplementRequest>();
    public EnrollmentConfirmation? EnrollmentConfirmation { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
