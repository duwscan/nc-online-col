using System;
using System.Collections.Generic;

namespace wnc.Features.Students.Applications.ViewModels;

public class StudentApplicationDocumentsViewModel
{
    public Guid ApplicationId { get; set; }
    public string PageTitle { get; set; } = "Hồ sơ đính kèm";
    public string CurrentStatus { get; set; } = "DRAFT";
    public bool IsComplete { get; set; }
    public List<DocumentRequirementViewModel> DocumentRequirements { get; set; } = [];
}

public class DocumentRequirementViewModel
{
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public int MaxFiles { get; set; }
    public bool RequiresNotarization { get; set; }
    public bool RequiresOriginalCopy { get; set; }
    public string? Notes { get; set; }
    public int UploadedCount { get; set; }
    public List<UploadedDocumentViewModel> UploadedDocuments { get; set; } = [];
}

public class UploadedDocumentViewModel
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedAt { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = "PENDING";
}
