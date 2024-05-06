using AutoMapper;
using LinkedInAuth.MVC.Models;
using Newtonsoft.Json.Linq;

namespace LinkedInAuth.MVC.Profiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<JObject, UserProfileViewModel>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src["email"].Value<string>()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src["name"].Value<string>()))
            .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src["picture"].Value<string>()));
    }
}