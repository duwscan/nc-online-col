using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.Rounds.ViewModels;

public class RoundMethodViewModel
{
    public Guid Id { get; set; }
    public Guid RoundProgramId { get; set; }
    public Guid MethodId { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public decimal? MinimumScore { get; set; }
    public string? CalculationRule { get; set; }
    public string? CombinationCode { get; set; }
    public string? PriorityPolicy { get; set; }
    public string Status { get; set; } = "ACTIVE";
}

public class EditRoundMethodsViewModel
{
    public Guid RoundId { get; set; }
    public string RoundCode { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public List<RoundProgramMethodsViewModel> Programs { get; set; } = [];
}

public class RoundProgramMethodsViewModel
{
    public Guid Id { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? MajorName { get; set; }
    public List<RoundMethodViewModel> Methods { get; set; } = [];
    public List<AdmissionMethodOption> AvailableMethods { get; set; } = [];
}

public class AdmissionMethodOption
{
    public Guid Id { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
}
