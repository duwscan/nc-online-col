using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Students.Authentication.ViewModels;

public class StudentLoginViewModel
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
