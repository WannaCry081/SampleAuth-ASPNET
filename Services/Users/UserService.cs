using AutoMapper;
using sample_auth_aspnet.Data;
using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Models.Response;
using sample_auth_aspnet.Models.Utils;

namespace sample_auth_aspnet.Services.Users;

public class UserService(
    DataContext context,
    IMapper mapper) : IUserService
{
    public async Task<ApiResponse<UserDetailsDto>> GetUserDetailsAsync(int id)
    {
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Id.Equals(id));

        var userDetails = mapper.Map<UserDetailsDto>(user);

        return ApiResponse<UserDetailsDto>.SuccessResponse(userDetails, Success.ENTITY_RETRIEVED("User Details"));
    }
}
