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
    JWTSettings jwt) : IAuthService
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

            var token = new Token
            {
                UserId = user.Id,
                Refresh = authDto.Refresh,
                User = user,
                Expiration = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(jwt.RefreshTokenExpiry))
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
        var token = new Token
        {
            UserId = user.Id,
            Refresh = authDto.Refresh,
            User = user,
            Expiration = DateTime.UtcNow.AddDays(
                Convert.ToDouble(jwt.RefreshTokenExpiry))
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

        var principal = TokenUtil.ValidateRefreshToken(refreshToken, jwt);
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

        var expClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
        if (string.IsNullOrEmpty(expClaim) ||
            !long.TryParse(expClaim, out var expSeconds) ||
            DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime < DateTime.UtcNow.AddMinutes(10))
        {
            var user = token.User;
            var newTokensGenerated = TokenUtil.GenerateTokens(user, jwt);

            token.IsRevoked = true;

            var newRefreshToken = new Token
            {
                UserId = user.Id,
                Refresh = newTokensGenerated.Refresh,
                Expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(jwt.RefreshTokenExpiry))
            };

            user.Tokens.Add(newRefreshToken);
            await context.Tokens.AddAsync(newRefreshToken);
            await context.SaveChangesAsync();

            return ApiResponse<AuthDto>.SuccessResponse(
                newTokensGenerated, Success.IS_AUTHENTICATED);
        }

        var newAccessToken = new AuthDto
        {
            Access = TokenUtil.GenerateAccess(token.User, jwt),
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