using AutoMapper;
using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Models.Entities;

namespace sample_auth_aspnet.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDetailsDto>();
    }
}
