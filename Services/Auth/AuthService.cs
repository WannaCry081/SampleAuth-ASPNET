using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
                User = user
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

        if (!BCrypt.Net.BCrypt.Verify(authLogin.Password, user.Password))
        {
            details.Add("password", "Invalid credentials");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized, details);
        }

        var authDto = new AuthDto
        {
            Access = TokenUtil.GenerateAccess(user, configuration),
            Refresh = TokenUtil.GenerateRefresh(user, configuration)
        };

        var token = new Token
        {
            UserId = user.Id,
            Refresh = authDto.Refresh,
            User = user
        };

        await context.Tokens.AddAsync(token);
        await context.SaveChangesAsync();
        return ApiResponse<AuthDto>.SuccessResponse(authDto, Success.IS_AUTHENTICATED());
    }

    public async Task<bool> LogoutUserAsync(string refreshToken)
    {
        var token = await context.Tokens.FirstOrDefaultAsync(
            t => t.Refresh.Equals(refreshToken));

        if (token is null)
            return false;

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<ApiResponse<AuthDto>> RefreshUserTokensAsync(string refreshToken)
    {
        var principal = TokenUtil.ValidateRefreshToken(refreshToken, configuration);

        if (principal == null)
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized);
        }

        var token = await context.Tokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
            t => t.Refresh.Equals(refreshToken));

        if (!(token != null && !token.IsRevoked))
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized);
        }

        var user = token.User;
        var authDto = new AuthDto { };
        var expClaim = principal.Claims.FirstOrDefault(
            c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

        if (DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim!)).UtcDateTime
            < DateTime.UtcNow.AddMinutes(10))
        {
            token.IsRevoked = true;

            authDto = new AuthDto
            {
                Access = TokenUtil.GenerateAccess(user, configuration),
                Refresh = TokenUtil.GenerateRefresh(user, configuration),
            };

            var newRefreshToken = new Token
            {
                UserId = user.Id,
                Refresh = authDto.Refresh,
                User = user
            };

            await context.Tokens.AddAsync(newRefreshToken);
            await context.SaveChangesAsync();
        }
        else
        {
            authDto = new AuthDto
            {
                Access = TokenUtil.GenerateAccess(user, configuration),
                Refresh = refreshToken
            };
        }

        return ApiResponse<AuthDto>.SuccessResponse(authDto, Success.IS_AUTHENTICATED());
    }
}
