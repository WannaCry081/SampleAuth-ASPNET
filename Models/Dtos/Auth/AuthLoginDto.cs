using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

public class AuthLoginDto
{
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; } = string.Empty;
};
