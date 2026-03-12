using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class DocumentType
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string DocumentCode { get; set; } = string.Empty;
    [MaxLength(255)] public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }

    public ICollection<RoundDocumentRequirement> RoundDocumentRequirements { get; set; } = new List<RoundDocumentRequirement>();
    public ICollection<ApplicationDocument> ApplicationDocuments { get; set; } = new List<ApplicationDocument>();
}
