using wnc.Models;

namespace wnc.Features.Admin.Rounds.ViewModels;

public class RoundDocumentRequirementCrudViewModel
{
    public Guid Id { get; set; }
    public Guid RoundProgramId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public bool IsRequired { get; set; }
    public bool RequiresNotarization { get; set; }
    public bool RequiresOriginalCopy { get; set; }
    public int MaxFiles { get; set; } = 1;
    public string? Notes { get; set; }
    public List<RoundProgram> RoundPrograms { get; set; } = [];
    public List<DocumentType> DocumentTypes { get; set; } = [];
}
