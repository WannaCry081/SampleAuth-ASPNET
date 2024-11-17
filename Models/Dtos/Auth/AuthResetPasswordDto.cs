using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

public class AuthResetPasswordDto
{
    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(255, ErrorMessage = "Password cannot exceeds {1} characters.")]
    [MinLength(8, ErrorMessage = "Password must be at least {1} characters.")]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "RePassword is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string RePassword { get; init; } = string.Empty;
}
