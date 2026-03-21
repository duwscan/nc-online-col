using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Students.Authentication.ViewModels;

public class StudentSignupViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập ngày sinh.")]
    [Display(Name = "Ngày sinh")]
    public DateOnly DateOfBirth { get; set; }

    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
    [Display(Name = "Email hoặc số điện thoại")]
    public string LoginIdentifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
