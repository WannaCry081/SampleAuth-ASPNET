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
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The password for the account.
    /// </summary>
    /// <example>StrongPassword123</example>
    public string Password { get; init; } = string.Empty;
};
