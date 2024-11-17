using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Dtos.Auth;

public class AuthRefreshTokenDto
{
    [Required(ErrorMessage = "Refresh Token is required.")]
    public string Refresh { get; init; } = string.Empty;
}
