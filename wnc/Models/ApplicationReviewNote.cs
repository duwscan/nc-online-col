using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class ApplicationReviewNote
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid AuthorUserId { get; set; }
    [MaxLength(30)] public string NoteType { get; set; } = "GENERAL";
    public string Content { get; set; } = string.Empty;
    public bool IsVisibleToCandidate { get; set; }
    public DateTime CreatedAt { get; set; }

    public AdmissionApplication Application { get; set; } = null!;
    public AppUser AuthorUser { get; set; } = null!;
}
