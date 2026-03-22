using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.Rounds.ViewModels;

public class RoundDocumentRequirementViewModel
{
    public Guid Id { get; set; }
    public Guid RoundProgramId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool RequiresNotarization { get; set; }
    public bool RequiresOriginalCopy { get; set; }
    public int MaxFiles { get; set; } = 1;
    public string? Notes { get; set; }
}

public class EditRoundDocumentsViewModel
{
    public Guid RoundId { get; set; }
    public string RoundCode { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public List<RoundProgramDocumentsViewModel> Programs { get; set; } = [];
}

public class RoundProgramDocumentsViewModel
{
    public Guid Id { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? MajorName { get; set; }
    public List<RoundDocumentRequirementViewModel> Requirements { get; set; } = [];
    public List<DocumentTypeOption> AvailableDocumentTypes { get; set; } = [];
}

public class DocumentTypeOption
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentCode { get; set; } = string.Empty;
}
