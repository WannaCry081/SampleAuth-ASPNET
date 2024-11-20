namespace sample_auth_aspnet.Models.Dtos.Users;

/// <summary>
///     DTO for retrieving user's details.
/// </summary>
public class UserDetailsDto
{
    /// <summary>
    ///     The email address of the user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    ///     The first name of the user.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    ///     The last name of the user.
    /// </summary>
    public string LastName { get; init; } = string.Empty;
}
