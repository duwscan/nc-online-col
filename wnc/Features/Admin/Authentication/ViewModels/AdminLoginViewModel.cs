using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.Authentication.ViewModels;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email hoặc số điện thoại.")]
    [Display(Name = "Email hoặc số điện thoại")]
    public string LoginIdentifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public string? ReturnUrl { get; set; }
}
