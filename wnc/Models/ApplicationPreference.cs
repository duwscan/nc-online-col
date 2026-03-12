using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class ApplicationPreference
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public int PriorityOrder { get; set; }
    public Guid ProgramId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? MethodId { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }

    public AdmissionApplication Application { get; set; } = null!;
    public TrainingProgram Program { get; set; } = null!;
    public Major? Major { get; set; }
    public AdmissionMethod? Method { get; set; }
}
