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

        return ApiResponse<AuthDto>.SuccessResponse(authDto, Success.IS_AUTHENTICATED());
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

        return ApiResponse<AuthDto>.SuccessResponse(authDto, Success.IS_AUTHENTICATED());
    }


    public async Task<ApiResponse<AuthDto>> RefreshUserTokensAsync(int id, string refreshToken)
    {
        var principal = TokenUtil.ValidateRefreshToken(
            refreshToken, configuration, out var validatedToken);

        if (principal == null || validatedToken is not JwtSecurityToken jwtToken)
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized);
        }

        if (jwtToken.Issuer != configuration["JWT:Issuer"] ||
            jwtToken.Audiences.Contains(configuration["JWT:Audience"]) == false)
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.Unauthorized, Error.ErrorType.Unauthorized);
        }

        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Id.Equals(id));

        var authDto = new AuthDto
        {
            Access = TokenUtil.GenerateAccess(user!, configuration),
            Refresh = TokenUtil.GenerateRefresh(user!, configuration)
        };

        return ApiResponse<AuthDto>.SuccessResponse(authDto, Success.IS_AUTHENTICATED());
    }
}
