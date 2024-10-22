using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Response;

namespace sample_auth_aspnet.Services.Auth;

public interface IAuthService
{
    Task<ApiResponse<AuthDto>> RegisterUserAsync(AuthRegisterDto authRegister);
}
