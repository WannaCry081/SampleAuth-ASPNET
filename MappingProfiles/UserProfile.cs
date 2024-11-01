using AutoMapper;
using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Models.Entities;

namespace sample_auth_aspnet.MappingProfiles;

/// <summary>
/// AutoMapper profile for user-related mappings
/// </summary>
public class UserProfile : Profile
{
    /// <summary>
    /// Initializes the mapping configuration for authentication DTOs and entities.
    /// </summary>
    public UserProfile()
    {
        CreateMap<User, UserDetailsDto>();
    }
}
