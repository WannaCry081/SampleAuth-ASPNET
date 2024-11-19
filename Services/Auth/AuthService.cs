using AutoMapper;
using sample_auth_aspnet.Data;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Entities;
using sample_auth_aspnet.Models.Response;
using sample_auth_aspnet.Services.Email;
using sample_auth_aspnet.Models.Utils;
using sample_auth_aspnet.Services.Utils;
using System.Security.Claims;

namespace sample_auth_aspnet.Services.Auth;
public class AuthService(
    IEmailService emailService,
    ILogger<AuthService> logger,
    DataContext context,
    IMapper mapper,
    JWTSettings jwt,
    ApplicationSettings app) : IAuthService
{
    public async Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister)
    {
        var details = new Dictionary<string, string>();
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if (await context.Users.AnyAsync(u => u.Email.Equals(authRegister.Email)))
            {
                details.Add("email", "Invalid email or already in use.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.ValidationError, Error.ErrorType.ValidationError, details);
            }

            var user = mapper.Map<User>(authRegister);
            user.Password = PasswordUtil.HashPassword(user.Password);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var authDto = TokenUtil.GenerateTokens(user, jwt);
            await SaveRefreshTokenAsync(user, authDto.Refresh, jwt.RefreshTokenExpiry);
            await transaction.CommitAsync();

            return ApiResponse<AuthDto>.SuccessResponse(
                authDto, Success.IS_AUTHENTICATED);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error in registering a new user.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.ERROR_CREATING_RESOURCE("User"), Error.ErrorType.InternalServer);
        }
    }

    public async Task<ApiResponse<AuthDto>> LoginUserAsync(AuthLoginDto authLogin)
    {
        var details = new Dictionary<string, string>();

        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Email.Equals(authLogin.Email));

        if (user is null || !PasswordUtil.VerifyPassword(
            user.Password, authLogin.Password))
        {
            details.Add("user", "Invalid user credentials.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized, details);
        }

        var authDto = TokenUtil.GenerateTokens(user, jwt);
        await SaveRefreshTokenAsync(user, authDto.Refresh, jwt.RefreshTokenExpiry);

        return ApiResponse<AuthDto>.SuccessResponse(
            authDto, Success.IS_AUTHENTICATED);
    }

    public async Task<bool> LogoutUserAsync(string refreshToken)
    {
        var token = await context.Tokens.FirstOrDefaultAsync(
            t => t.Refresh.Equals(refreshToken) && !t.IsRevoked && t.Expiration >= DateTime.UtcNow);

        if (token is null)
            return false;

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<ApiResponse<AuthDto>> RefreshUserTokensAsync(string refreshToken)
    {
        var details = new Dictionary<string, string>();
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var principal = TokenUtil.ValidateToken(refreshToken, jwt);
            if (principal is null)
            {
                details.Add("token", "Invalid refresh token.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.Unauthorized, Error.ErrorType.Unauthorized, details);
            }

            var token = await context.Tokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Refresh == refreshToken);

            if (token is null || token.IsRevoked || token.Expiration < DateTime.UtcNow)
            {
                details.Add("token", "Refresh token is already expired or invalid.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.Unauthorized, Error.ErrorType.Unauthorized, details);
            }

            if (!TokenUtil.IsTokenNearExpiration(principal, bufferMinutes: 10))
            {
                var newAccessToken = new AuthDto
                {
                    Refresh = refreshToken,
                    Access = TokenUtil.GenerateToken(
                        token.User, jwt, TokenUtil.TokenType.ACCESS)
                };

                return ApiResponse<AuthDto>.SuccessResponse(newAccessToken, Success.IS_AUTHENTICATED);
            }

            var user = token.User;
            token.IsRevoked = true;

            var authDto = TokenUtil.GenerateTokens(user, jwt);
            await SaveRefreshTokenAsync(user, authDto.Refresh, jwt.RefreshTokenExpiry);
            await transaction.CommitAsync();

            return ApiResponse<AuthDto>.SuccessResponse(authDto, Success.IS_AUTHENTICATED);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error refreshing user's token.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.ERROR_CREATING_RESOURCE("Token"), Error.ErrorType.InternalServer);
        }
    }

    public async Task<ApiResponse<object?>> ForgotUserPasswordAsync(string email)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email.Equals(email));

        if (user is not null)
        {
            var resetToken = TokenUtil.GenerateToken(user, jwt, TokenUtil.TokenType.RESET);
            var resetLink = $"{app.Url}?token={resetToken}";

            await emailService.SendResetEmailAsync(email, resetLink);
        }

        return ApiResponse<object?>.SuccessResponse(null, Success.EMAILED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<AuthDto>> ResetUserPasswordAsync(
    string resetToken, AuthResetPasswordDto authResetPassword)
    {
        const string ResetPasswordPurpose = "reset-password";
        var details = new Dictionary<string, string>();
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {

            var principal = TokenUtil.ValidateToken(resetToken, jwt);
            if (principal is null)
            {
                details.Add("token", "Invalid or expired reset token.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.Unauthorized, Error.ErrorType.Unauthorized, details);
            }

            var purposeClaim = principal.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
            if (string.IsNullOrEmpty(purposeClaim) || purposeClaim != ResetPasswordPurpose)
            {
                details.Add("token", "Invalid token purpose.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.Unauthorized, Error.ErrorType.Unauthorized, details);
            }

            var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim))
            {
                details.Add("token", "Invalid email information in reset token.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.Unauthorized, Error.ErrorType.Unauthorized, details);
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(emailClaim));
            if (user is null)
            {
                details.Add("token", "User not found.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.Unauthorized, Error.ErrorType.Unauthorized, details);
            }

            user.Password = PasswordUtil.HashPassword(authResetPassword.Password);
            await context.SaveChangesAsync();

            var authDto = TokenUtil.GenerateTokens(user, jwt);
            await SaveRefreshTokenAsync(user, authDto.Refresh, jwt.RefreshTokenExpiry);
            await transaction.CommitAsync();

            return ApiResponse<AuthDto>.SuccessResponse(
                authDto, Success.IS_AUTHENTICATED);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error resettings user's password.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.ERROR_UPDATING_RESOURCE("User"), Error.ErrorType.InternalServer);
        }
    }

    public async Task RemoveRevokedTokenAsync()
    {
        await context.Tokens
            .Where(t => t.IsRevoked || t.Expiration < DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }

    private async Task SaveRefreshTokenAsync(User user, string refreshToken, int expiryDays)
    {
        var token = new Token
        {
            UserId = user.Id,
            Refresh = refreshToken,
            Expiration = DateTime.UtcNow.AddDays(expiryDays)
        };

        user.Tokens.Add(token);
        await context.Tokens.AddAsync(token);
        await context.SaveChangesAsync();
    }
}