using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Service.Mappers.Profiles;

namespace ZODs.Api.Service.Mappers
{
    public static class SnippetTriggerPrefixMapper
    {
        static SnippetTriggerPrefixMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SnippetTriggerPrefixMapperProfile>()).CreateMapper();
        }

        public static IMapper Mapper { get; }

        /// <summary>
        /// Maps <see cref="SnippetTriggerPrefixInputDto"/> to <see cref="SnippetTriggerPrefix"/>.
        /// </summary>
        /// <param name="SnippetTriggerPrefix">Entity to map from.</param>
        /// <returns>Mapped <see cref="SnippetTriggerPrefixDto"/>.</returns>
        public static SnippetTriggerPrefix ToEntity(this SnippetTriggerPrefixInputDto inputDto)
        {
            return Mapper.Map<SnippetTriggerPrefix>(inputDto);
        }

        public static SnippetTriggerPrefixDto ToDto(this SnippetTriggerPrefix entity)
        {
            return Mapper.Map<SnippetTriggerPrefixDto>(entity);
        }

        public static PagedResponse<SnippetTriggerPrefixDto> ToPagedResponse(this PagedEntities<SnippetTriggerPrefix> pagedEntities)
        {
            return Mapper.Map<PagedResponse<SnippetTriggerPrefixDto>>(pagedEntities);
        }
    }
}
