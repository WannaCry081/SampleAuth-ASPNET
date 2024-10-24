using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
    /// Registers a new user and returns access and refresh tokens.
    /// </summary>
    /// <param name="authRegister">The user registration details</param>
    /// <returns>An access and refresh tokens if registration is successful</returns>
    public async Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister)
    {
        if (!authRegister.Password.Equals(authRegister.RePassword))
            return ApiResponse<AuthDto>.ErrorResponse(
                "Passwords do not match.", Errors.ErrorType.BadRequest);

        var isUserExists = await context.Users.FirstOrDefaultAsync(
            u => u.Email.Equals(authRegister.Email)
        );

        if (isUserExists != null)
            return ApiResponse<AuthDto>.ErrorResponse(
                $"A user with this email already exists.", Errors.ErrorType.BadRequest);

        var user = mapper.Map<User>(authRegister);
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var authDto = new AuthDto
        {
            Access = TokenUtil.GenerateAccess(user, configuration),
            Refresh = TokenUtil.GenerateRefresh(user, configuration)
        };

        return ApiResponse<AuthDto>.SuccessResponse(authDto);
    }

}
