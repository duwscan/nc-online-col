using System;

namespace wnc.Features.Students.Applications.ViewModels;

public class StudentApplicationsIndexViewModel
{
    public string PageTitle { get; set; } = "Danh sách đơn đăng ký";
    public List<ApplicationSummary> Applications { get; set; } = [];
}

public class ApplicationSummary
{
    public Guid Id { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string? MajorName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public bool IsDraft => SubmittedAt == null;
}
