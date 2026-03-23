using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.Rounds.ViewModels;

public class CreateRoundViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Mã đợt xét tuyển là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã đợt xét tuyển không được vượt quá 50 ký tự.")]
    [Display(Name = "Mã đợt xét tuyển")]
    public string RoundCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên đợt xét tuyển là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tên đợt xét tuyển không được vượt quá 255 ký tự.")]
    [Display(Name = "Tên đợt xét tuyển")]
    public string RoundName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Năm tuyển sinh là bắt buộc.")]
    [Range(2000, 3000, ErrorMessage = "Năm tuyển sinh không hợp lệ.")]
    [Display(Name = "Năm tuyển sinh")]
    public int AdmissionYear { get; set; } = DateTime.UtcNow.Year;

    [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
    [Display(Name = "Thời gian bắt đầu")]
    public DateTime StartAt { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
    [Display(Name = "Thời gian kết thúc")]
    public DateTime EndAt { get; set; } = DateTime.UtcNow.AddDays(1);

    [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "DRAFT";

    [Display(Name = "Ghi chú")]
    public string? Notes { get; set; }

    [Display(Name = "Cho phép xác nhận nhập học")]
    public bool AllowEnrollmentConfirmation { get; set; }

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["DRAFT", "PUBLISHED", "CLOSED"];

    public List<AdmissionMethodOption> AvailableMethods { get; set; } = [];
    public List<Guid> SelectedMethodIds { get; set; } = [];
    public List<DocumentTypeOption> AvailableDocumentTypes { get; set; } = [];
    public List<Guid> SelectedDocumentTypeIds { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt >= EndAt)
        {
            yield return new ValidationResult(
                "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.",
                [nameof(StartAt), nameof(EndAt)]);
        }
    }
}
