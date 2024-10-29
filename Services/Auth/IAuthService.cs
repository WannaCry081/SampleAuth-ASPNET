using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Response;

namespace sample_auth_aspnet.Services.Auth;

/// <summary>
/// Service interface for authentication operation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and returns access and refresh tokens.
    /// </summary>
    /// <param name="authRegister">The user registration details</param>
    /// <returns>An access and refresh tokens if registration is successful</returns>
    Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister);

    /// <summary>
    /// Login a registerd user and returns the access and refresh tokens.
    /// </summary>
    /// <param name="authLogin">The user email and password</param>
    /// <returns>The access and refresh tokens if user successfully logged in</returns>
    Task<ApiResponse<AuthDto>> LoginUserAsync(AuthLoginDto authLogin);

    Task<bool> LogoutUserAsync(string refreshToken);

    /// <summary>
    /// Refreshes the authenticated user's access and refresh tokens.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="refreshToken">The user refresh token</param>
    /// <returns>The access and refresh tokens</returns>
    Task<ApiResponse<AuthDto>> RefreshUserTokensAsync(int id, string refreshToken);
}
