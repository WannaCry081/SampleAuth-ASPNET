using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Response;

namespace sample_auth_aspnet.Services.Auth;

public interface IAuthService
{
    /// <summary>
    ///     Registers a new user and returns access and refresh tokens.
    /// </summary>
    /// <param name="authRegister"></param>
    /// <returns>
    ///     An access and refresh tokens if registration is successful.
    /// </returns>
    Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister);

    /// <summary>
    ///     Login a registerd user and returns the access and refresh tokens.
    /// </summary>
    /// <param name="authLogin"></param>
    /// <returns>
    ///     The access and refresh tokens if user successfully logged in.
    /// </returns>
    Task<ApiResponse<AuthDto>> LoginUserAsync(AuthLoginDto authLogin);

    /// <summary>
    ///     Logout a user by invalidating their refresh token.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns>
    ///     Return a boolean if the process is successful.
    /// </returns>
    Task<bool> LogoutUserAsync(string refreshToken);

    /// <summary>
    ///     Refreshes the authenticated user's access and refresh tokens.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns>
    ///     The access and refresh tokens.
    /// </returns>
    Task<ApiResponse<AuthDto>> RefreshUserTokensAsync(string refreshToken);

    /// <summary>
    ///     Send an reset password email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>
    ///     Confirmation message if the SMTP send an email.
    /// </returns>
    Task<ApiResponse<object?>> ForgotUserPasswordAsync(string email);

    /// <summary>
    ///     Reset user's password.
    /// </summary>
    /// <param name="resetToken"></param>
    /// <param name="authResetPassword"></param>
    /// <returns>
    ///     An access and refresh tokens if password reset is successful.
    /// </returns>
    Task<ApiResponse<AuthDto>> ResetUserPasswordAsync(
        string resetToken, AuthResetPasswordDto authResetPassword);

    /// <summary>
    ///     Remove user's revoked tokens.
    /// </summary>
    Task RemoveRevokedTokenAsync();
}
