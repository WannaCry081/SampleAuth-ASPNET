using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

public class AuthRegisterDto
{
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; init; } = string.Empty;

    [MaxLength(50)]
    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; init; } = string.Empty;

    [MaxLength(50)]
    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Re-Password is required")]
    public string RePassword { get; init; } = string.Empty;
}
