using AutoMapper;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Entities;

namespace sample_auth_aspnet.MappingProfiles;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<AuthRegisterDto, User>()
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
    }
}
