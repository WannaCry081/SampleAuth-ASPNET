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

/// <summary>
/// Service class for authentication operation.
/// </summary>
public class AuthService(
    ILogger<AuthService> logger,
    DataContext context,
    IMapper mapper,
    IConfiguration configuration
) : IAuthService
{
    /// <summary>
    /// Registers a new user and returns the access and refresh tokens.
    /// </summary>
    /// <param name="authRegister">The user registration details</param>
    /// <returns>The access and refresh tokens if registration is successful</returns>
    public async Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister)
    {
        Dictionary<string, string> details = [];
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if (!authRegister.Password.Equals(authRegister.RePassword))
            {
                details.Add("password", "Password does not match");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.ValidationError, Error.ErrorType.ValidationError, details);
            }

            var isUserExists = await context.Users.FirstOrDefaultAsync(
                u => u.Email.Equals(authRegister.Email)
            );

            if (isUserExists != null)
            {
                details.Add("email", "Invalid email address");
                return ApiResponse<AuthDto>.ErrorResponse(
                    Error.ValidationError, Error.ErrorType.ValidationError, details);
            }

            var user = mapper.Map<User>(authRegister);
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

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

            user.Tokens.Add(token);

            await context.Tokens.AddAsync(token);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<AuthDto>.SuccessResponse(authDto, Success.IS_AUTHENTICATED());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error in registering a new user.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.ERROR_CREATING_RESOURCE("User"), Error.ErrorType.InternalServer);
        }
    }

    /// <summary>
    /// Login a registerd user and returns the access and refresh tokens.
    /// </summary>
    /// <param name="authLogin">The user email and password</param>
    /// <returns>The access and refresh tokens if user successfully logged in</returns>
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

    /// <summary>
    /// Refreshes the authenticated user's access and refresh tokens.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="refreshToken">The user refresh token</param>
    /// <returns>The access and refresh tokens</returns>
    public async Task<ApiResponse<AuthDto>> RefreshUserTokensAsync(int id, string refreshToken)
    {
        var principal = TokenUtil.ValidateRefreshToken(refreshToken, configuration);

        if (principal == null)
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized);
        }

        var token = await context.Tokens.FirstOrDefaultAsync(
            t => t.Refresh.Equals(refreshToken));

        if (!(token != null && !token.IsRevoked))
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized);
        }

        var authDto = new AuthDto { };
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Id.Equals(id));

        if (user == null)
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.NotFound, Error.ErrorType.NotFound);
        }

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
