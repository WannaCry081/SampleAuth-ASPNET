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

    Task<ApiResponse<AuthDto>> LoginUserAsync(AuthLoginDto authLogin);
}
