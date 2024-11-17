using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

/// <summary>
///     DTO used for getting refresh tokens.
/// </summary>
public class AuthRefreshTokenDto
{
    /// <summary>
    ///     The refresh token of the user.
    /// </summary>
    [Required(ErrorMessage = "Refresh Token is required.")]
    public string Refresh { get; init; } = string.Empty;
}
