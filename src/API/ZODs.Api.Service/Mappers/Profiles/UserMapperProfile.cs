using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.Dtos.User;

namespace ZODs.Api.Service.Mappers.Profiles;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        this.CreateMap<User, UserDto>();
        this.CreateMap<UserDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(x => x.Email))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        this.CreateMap<SignUpDto, UserDto>();

        this.CreateMap<PagedEntities<User>, PagedResponse<UserDto>>();

    }
}
