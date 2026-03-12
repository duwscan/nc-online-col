using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class RoundDocumentRequirement
{
    public Guid Id { get; set; }
    public Guid RoundProgramId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool RequiresNotarization { get; set; }
    public bool RequiresOriginalCopy { get; set; }
    public int MaxFiles { get; set; } = 1;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public RoundProgram RoundProgram { get; set; } = null!;
    public DocumentType DocumentType { get; set; } = null!;
}
