using AutoMapper;
using Microsoft.EntityFrameworkCore;
using sample_auth_aspnet.Data;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Entities;
using sample_auth_aspnet.Models.Response;
using sample_auth_aspnet.Models.Utils;
using sample_auth_aspnet.Services.Utils;

namespace sample_auth_aspnet.Services.Auth;

public class AuthService(
    DataContext context,
    IMapper mapper,
    IConfiguration configuration
) : IAuthService
{
    public async Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister)
    {
        if (!authRegister.Password.Equals(authRegister.RePassword))
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                Errors.Unauthorized, Errors.ErrorType.BadRequest);
        }

        var isUserExists = await context.Users.FirstOrDefaultAsync(
            u => u.Email.Equals(authRegister.Email)
        );

        if (isUserExists != null)
        {
            return ApiResponse<AuthDto>.ErrorResponse(
                $"User {Errors.AlreadyExists.ToLower()}", Errors.ErrorType.BadRequest);
        }

        var user = mapper.Map<User>(authRegister);
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        return ApiResponse<AuthDto>.SuccessResponse(
            new AuthDto
            {
                Access = TokenUtil.GenerateAccess(user, configuration),
                Refresh = TokenUtil.GenerateRefresh(user, configuration)
            }
        );
    }
}
