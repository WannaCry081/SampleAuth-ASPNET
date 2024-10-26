using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Models.Response;

namespace sample_auth_aspnet.Services.Users;

public interface IUserService
{
    Task<ApiResponse<UserDetailsDto>> GetUserDetailsAsync(int userId);
}
