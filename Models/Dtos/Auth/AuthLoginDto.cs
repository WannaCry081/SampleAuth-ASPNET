using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

/// <summary>
/// DTO used for login a user.
/// </summary>
public class AuthLoginDto
{
    /// <summary>
    /// The email address of the user.
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The password for the account.
    /// </summary>
    /// <example>StrongPassword123</example>
    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(255, ErrorMessage = "Password cannot exceeds {1} characters.")]
    public string Password { get; init; } = string.Empty;
};
