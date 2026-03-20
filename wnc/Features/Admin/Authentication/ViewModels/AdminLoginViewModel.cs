using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.Authentication.ViewModels;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Please enter your email address or phone number.")]
    [Display(Name = "Email or phone number")]
    public string LoginIdentifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public string? ReturnUrl { get; set; }
}
