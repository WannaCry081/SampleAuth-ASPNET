namespace sample_auth_aspnet.Models.Dtos.Auth;

/// <summary>
/// DTO for access and refresh tokens.
/// </summary>
public class AuthDto
{
    /// <summary>
    /// Short-lived access token for API authentication.
    /// </summary>
    public string Access { get; init; } = null!;

    /// <summary>
    /// Long-lived refresh token for renewing access.
    /// </summary>
    public string Refresh { get; init; } = null!;
}
