using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.MajorManagement.ViewModels;

public class MajorFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã ngành.")]
    [StringLength(50, ErrorMessage = "Mã ngành không được vượt quá 50 ký tự.")]
    [Display(Name = "Mã ngành")]
    public string MajorCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên ngành.")]
    [StringLength(255, ErrorMessage = "Tên ngành không được vượt quá 255 ký tự.")]
    [Display(Name = "Tên ngành")]
    public string MajorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn chương trình đào tạo.")]
    [Display(Name = "Chương trình đào tạo")]
    public Guid? ProgramId { get; set; }

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Chỉ tiêu phải lớn hơn hoặc bằng 0.")]
    [Display(Name = "Chỉ tiêu")]
    public int Quota { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải lớn hơn hoặc bằng 0.")]
    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "ACTIVE";

    public bool IsEditMode { get; set; }

    public string ProgramName { get; set; } = string.Empty;

    public IReadOnlyList<MajorProgramOptionViewModel> AvailablePrograms { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["ACTIVE", "INACTIVE"];
}
