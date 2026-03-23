using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class RoundProgram
{
    public Guid Id { get; set; }
    public Guid RoundId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid? MajorId { get; set; }
    public int Quota { get; set; }
    public int? PublishedQuota { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AdmissionRound Round { get; set; } = null!;
    public TrainingProgram Program { get; set; } = null!;
    public Major? Major { get; set; }
    public ICollection<RoundAdmissionMethod> AdmissionMethods { get; set; } = new List<RoundAdmissionMethod>();
    public ICollection<RoundDocumentRequirement> DocumentRequirements { get; set; } = new List<RoundDocumentRequirement>();
    public ICollection<AdmissionApplication> AdmissionApplications { get; set; } = new List<AdmissionApplication>();
}
