using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

/// <summary>
///     DTO for forgot password.
/// </summary>
public class AuthForgotPasswordDto
{
    /// <summary>
    ///     The email address of the user.
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; init; } = string.Empty;
}
