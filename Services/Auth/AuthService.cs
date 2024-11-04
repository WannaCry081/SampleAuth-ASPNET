using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using sample_auth_aspnet.Data;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Entities;
using sample_auth_aspnet.Models.Response;
using sample_auth_aspnet.Models.Utils;
using sample_auth_aspnet.Services.Utils;

namespace sample_auth_aspnet.Services.Auth;

public class AuthService(
    ILogger<AuthService> logger,
    DataContext context,
    IMapper mapper,
    IConfiguration configuration
) : IAuthService
{
    public async Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister)
    {
        Dictionary<string, string> details = [];
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if (!authRegister.Password.Equals(authRegister.RePassword))
            {
                details.Add("rePassword", "Passwords do not match.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.ValidationError, Error.ErrorType.ValidationError, details);
            }

            if (await context.Users.AnyAsync(u => u.Email.Equals(authRegister.Email)))
            {
                details.Add("email", "Invalid email address.");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.ValidationError, Error.ErrorType.ValidationError, details);
            }

            var user = mapper.Map<User>(authRegister);
            user.Password = PasswordUtil.HashPassword(user.Password);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var authDto = TokenUtil.GenerateTokens(user, configuration);

            var token = new Token
            {
                UserId = user.Id,
                Refresh = authDto.Refresh,
                User = user,
                Expiration = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(configuration["JWT:RefreshTokenExpiry"]))
            };

            user.Tokens.Add(token);
            await context.Tokens.AddAsync(token);
            await context.SaveChangesAsync();
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
        Dictionary<string, string> details = [];

        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Email.Equals(authLogin.Email));

        if (user == null)
        {
            details.Add("email", "Invalid credentials");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized, details);
        }

        if (!PasswordUtil.VerifyPassword(user.Password, authLogin.Password))
        {
            details.Add("password", "Invalid credentials");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized, details);
        }

        var authDto = TokenUtil.GenerateTokens(user, configuration);
        var token = new Token
        {
            UserId = user.Id,
            Refresh = authDto.Refresh,
            User = user,
            Expiration = DateTime.UtcNow.AddDays(
                Convert.ToDouble(configuration["JWT:RefreshTokenExpiry"]))
        };

        await context.Tokens.AddAsync(token);
        await context.SaveChangesAsync();
        return ApiResponse<AuthDto>.SuccessResponse(
            authDto, Success.IS_AUTHENTICATED);
    }

    public async Task<bool> LogoutUserAsync(string refreshToken)
    {
        var token = await context.Tokens.FirstOrDefaultAsync(
            t => t.Refresh.Equals(refreshToken));

        if (token is null || token.IsRevoked || token.Expiration < DateTime.UtcNow)
            return false;

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<ApiResponse<AuthDto>> RefreshUserTokensAsync(string refreshToken)
    {
        var details = new Dictionary<string, string>();

        var principal = TokenUtil.ValidateRefreshToken(refreshToken, configuration);
        if (principal == null)
        {
            details.Add("token", "Invalid refresh token.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized, details);
        }

        var token = await context.Tokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Refresh == refreshToken);

        if (token == null || token.IsRevoked || token.Expiration < DateTime.UtcNow)
        {
            details.Add("token", "Refresh token is already expired or invalid.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized, details);
        }

        var expClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
        if (string.IsNullOrEmpty(expClaim) ||
            !long.TryParse(expClaim, out var expSeconds) ||
            DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime < DateTime.UtcNow.AddMinutes(10))
        {
            var user = token.User;
            var newTokensGenerated = TokenUtil.GenerateTokens(user, configuration);

            token.IsRevoked = true;

            var newRefreshToken = new Token
            {
                UserId = user.Id,
                Refresh = newTokensGenerated.Refresh,
                Expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration["JWT:RefreshTokenExpiry"]))
            };

            user.Tokens.Add(newRefreshToken);
            await context.Tokens.AddAsync(newRefreshToken);
            await context.SaveChangesAsync();

            return ApiResponse<AuthDto>.SuccessResponse(
                newTokensGenerated, Success.IS_AUTHENTICATED);
        }

        var newAccessToken = new AuthDto
        {
            Access = TokenUtil.GenerateAccess(token.User, configuration),
            Refresh = refreshToken
        };

        return ApiResponse<AuthDto>.SuccessResponse(
            newAccessToken, Success.IS_AUTHENTICATED);
    }

    public async Task RemoveRevokedTokenAsync()
    {
        var tokens = await context.Tokens
            .Where(t => t.IsRevoked || t.Expiration < DateTime.UtcNow)
            .ToListAsync();

        if (tokens.Count != 0)
        {
            context.Tokens.RemoveRange(tokens);
            await context.SaveChangesAsync();
        }
    }
}