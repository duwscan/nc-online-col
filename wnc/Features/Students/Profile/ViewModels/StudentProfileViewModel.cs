using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Students.Profile.ViewModels;

public class StudentProfileViewModel
{
    [Required]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Ngày sinh")]
    public DateOnly DateOfBirth { get; set; }

    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [Display(Name = "Số CCCD")]
    public string? NationalId { get; set; }

    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Địa chỉ")]
    public string? AddressLine { get; set; }

    [Display(Name = "Phường/Xã")]
    public string? Ward { get; set; }

    [Display(Name = "Quận/Huyện")]
    public string? District { get; set; }

    [Display(Name = "Tỉnh/Thành phố")]
    public string? ProvinceCode { get; set; }

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }
}
