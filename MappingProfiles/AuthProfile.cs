using AutoMapper;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Entities;

namespace sample_auth_aspnet.MappingProfiles;

/// <summary>
/// AutoMapper profile for authentication-related mappings.
/// </summary>
public class AuthProfile : Profile
{
    /// <summary>
    /// Initializes the mapping configuration for authentication DTOs and entities.
    /// </summary>
    public AuthProfile()
    {
        CreateMap<AuthRegisterDto, User>()
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
    }
}
