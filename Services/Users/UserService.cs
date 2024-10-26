using AutoMapper;
using Microsoft.EntityFrameworkCore;
using sample_auth_aspnet.Data;
using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Models.Response;
using sample_auth_aspnet.Models.Utils;

namespace sample_auth_aspnet.Services.Users;

/// <summary>
/// Service interface for authenticated user operation
/// </summary>
public class UserService(
    DataContext context,
    IMapper mapper) : IUserService
{
    /// <summary>
    /// Fetches the authenticated user's details.
    /// </summary>
    /// <param name="userId">The user ID from the access token</param>
    /// <returns>The user's details of the authenticated users</returns>
    public async Task<ApiResponse<UserDetailsDto>> GetUserDetailsAsync(int userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Id.Equals(userId));

        var userDetails = mapper.Map<UserDetailsDto>(user);

        return ApiResponse<UserDetailsDto>.SuccessResponse(userDetails, Success.ENTITY_RETRIEVED("User Details"));
    }
}
