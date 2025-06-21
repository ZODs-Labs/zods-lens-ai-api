using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos.AILens;
using ZODs.Api.Repository.Entities.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Service.Mappers.Profiles;

namespace ZODs.Api.Service.Mappers
{
    public static class AILensMapper
    {

        static AILensMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<AILensMapperProfile>()).CreateMapper();
        }

        public static IMapper Mapper { get; }

        public static PagedResponse<UserAILensDto> ToPagedResponse(this PagedEntities<UserAILensDto> pagedEntities)
        {
            return Mapper.Map<PagedResponse<UserAILensDto>>(pagedEntities);
        }

        public static AILensDto ToDto(this AILens entity)
        {
            return Mapper.Map<AILensDto>(entity);
        }

        public static AILens ToEntity(this AILensInputDto inputDto)
        {
            return Mapper.Map<AILens>(inputDto);
        }
    }
}