using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class UserViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [MaxLength(100)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "ACTIVE";

    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? PhoneVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<string> Roles { get; set; } = new();
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(100)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "ACTIVE";

    public List<string> SelectedRoles { get; set; } = new();
}

public class EditUserViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [MaxLength(100)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "ACTIVE";

    public List<string> SelectedRoles { get; set; } = new();
}

public class UserSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserPagedResult
{
    public List<UserViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
