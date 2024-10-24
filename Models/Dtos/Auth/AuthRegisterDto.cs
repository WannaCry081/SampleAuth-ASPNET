namespace sample_auth_aspnet.Models.Dtos.Auth;

/// <summary>
/// DTO used to register a new user.
/// </summary>
public class AuthRegisterDto
{
    /// <summary>
    /// The email address of the user.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The first name of the user.
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// The last name of the user.
    /// </summary>
    /// <example>Doe</example>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// The password for the account.
    /// </summary>
    /// <example>StrongPassword123</example>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// The password confirmation.
    /// </summary>
    /// <example>StrongPassword123</example>
    public string RePassword { get; init; } = string.Empty;
}
