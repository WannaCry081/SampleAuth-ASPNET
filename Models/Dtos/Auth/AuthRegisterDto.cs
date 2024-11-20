using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

/// <summary>
///     DTO used to register a new user.
/// </summary>
public class AuthRegisterDto
{
    /// <summary>
    ///     The email address of the user.
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    ///     The first name of the user.
    /// </summary>
    /// <example>John</example>
    [MaxLength(50, ErrorMessage = "First name cannot exceed {1} characters.")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    ///     The last name of the user.
    /// </summary>
    /// <example>Doe</example>
    [MaxLength(50, ErrorMessage = "Last name cannot exceed {1} characters.")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    ///     The password for the account.
    /// </summary>
    /// <example>StrongPassword123</example>
    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(255, ErrorMessage = "Password cannot exceed {1} characters.")]
    [MinLength(8, ErrorMessage = "Password must be at least {1} characters.")]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    ///     The password confirmation.
    /// </summary>
    /// <example>StrongPassword123</example>
    [Required(ErrorMessage = "RePassword is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string RePassword { get; init; } = string.Empty;
}
