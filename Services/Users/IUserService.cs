using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Models.Response;

namespace sample_auth_aspnet.Services.Users;

public interface IUserService
{
    /// <summary>
    ///     Fetches the authenticated user's details.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>
    ///     The user's details of the authenticated users
    /// </returns>
    Task<ApiResponse<UserDetailsDto>> GetUserDetailsAsync(int id);
}
