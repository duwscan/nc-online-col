using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.TrainingProgramManagement.ViewModels;

public class TrainingProgramFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã chương trình.")]
    [StringLength(50, ErrorMessage = "Mã chương trình không được vượt quá 50 ký tự.")]
    [Display(Name = "Mã chương trình")]
    public string ProgramCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên chương trình.")]
    [StringLength(255, ErrorMessage = "Tên chương trình không được vượt quá 255 ký tự.")]
    [Display(Name = "Tên chương trình")]
    public string ProgramName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn hệ đào tạo.")]
    [Display(Name = "Hệ đào tạo")]
    public string EducationType { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Học phí phải lớn hơn hoặc bằng 0.")]
    [Display(Name = "Học phí")]
    public decimal? TuitionFee { get; set; }

    [StringLength(100, ErrorMessage = "Thời gian đào tạo không được vượt quá 100 ký tự.")]
    [Display(Name = "Thời gian đào tạo")]
    public string? DurationText { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Chỉ tiêu phải lớn hơn hoặc bằng 0.")]
    [Display(Name = "Chỉ tiêu")]
    public int Quota { get; set; }

    [StringLength(255, ErrorMessage = "Đơn vị quản lý không được vượt quá 255 ký tự.")]
    [Display(Name = "Đơn vị quản lý")]
    public string? ManagingUnit { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "ACTIVE";

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải lớn hơn hoặc bằng 0.")]
    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; }

    public bool IsEditMode { get; set; }

    public IReadOnlyList<TrainingProgramFilterOptionViewModel> AvailableEducationTypes { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["ACTIVE", "INACTIVE"];
}
