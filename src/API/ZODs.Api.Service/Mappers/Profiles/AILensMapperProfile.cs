using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos.AILens;
using ZODs.Api.Repository.Entities.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;

namespace ZODs.Api.Service.Mappers.Profiles;

public sealed class AILensMapperProfile : Profile
{
    public AILensMapperProfile()
    {
        this.CreateMap<AILens, AILensDto>();
        this.CreateMap<AILensInputDto, AILens>();

        this.CreateMap<PagedEntities<UserAILensDto>, PagedResponse<UserAILensDto>>();
    }
}