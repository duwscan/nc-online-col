using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class ApplicationDocument
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid DocumentTypeId { get; set; }
    [MaxLength(255)] public string FileName { get; set; } = string.Empty;
    [MaxLength(500)] public string StoragePath { get; set; } = string.Empty;
    [MaxLength(100)] public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    [MaxLength(128)] public string? Checksum { get; set; }
    public Guid? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    [MaxLength(30)] public string ValidationStatus { get; set; } = "PENDING";
    public bool IsLatest { get; set; } = true;

    public AdmissionApplication Application { get; set; } = null!;
    public DocumentType DocumentType { get; set; } = null!;
    public AppUser? UploadedByUser { get; set; }
}
