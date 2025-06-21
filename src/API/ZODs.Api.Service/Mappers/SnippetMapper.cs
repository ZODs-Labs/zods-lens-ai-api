using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos.Snippet;
using ZODs.Api.Service.Mappers.Profiles;

namespace ZODs.Api.Service.Mappers
{
    public static class SnippetMapper
    {
        static SnippetMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SnippetMapperProfile>()).CreateMapper();
        }

        public static IMapper Mapper { get; }

        /// <summary>
        /// Maps <see cref="Snippet"/> to <see cref="SnippetDto"/>.
        /// </summary>
        /// <param name="Snippet">Entity to map from.</param>
        /// <returns>Mapped <see cref="SnippetDto"/>.</returns>
        public static SnippetDto ToDto(this Snippet Snippet)
        {
            return Mapper.Map<SnippetDto>(Snippet);
        }

        /// <summary>
        /// Maps collection of <see cref="Snippet"/> to collection of <see cref="SnippetDto"/>.
        /// </summary>
        /// <param name="entities">Collection of entities to map from.</param>
        /// <returns>Collection of mapped <see cref="SnippetDto"/>.</returns>
        public static IEnumerable<SnippetDto> ToDto(this IEnumerable<Snippet> entities)
        {
            return Mapper.Map<IEnumerable<SnippetDto>>(entities);
        }

        /// <summary>
        /// Maps <see cref="SnippetDto"/> to <see cref="Snippet"/>.
        /// </summary>
        /// <param name="SnippetDto">Dto to map from.</param>
        /// <returns>Mapped entity.</returns>
        public static Snippet ToEntity(this SnippetDto SnippetDto, Guid? id = null)
        {
            var entity = Mapper.Map<Snippet>(SnippetDto);
            if (entity != null && id != null)
            {
                entity.Id = id.Value;
            }

            return entity!;
        }

        public static Snippet ToEntity(this UpsertSnippetInputDto dto)
        {
            return Mapper.Map<Snippet>(dto);
        }

        /// <summary>
        /// Maps collection of <see cref="SnippetDto"/> to collection of <see cref="Snippet"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <returns>Paged response.</returns>
        public static PagedResponse<SnippetDto> ToPagedResponse(this PagedEntities<Snippet> entities)
        {
            return Mapper.Map<PagedResponse<SnippetDto>>(entities);
        }

        /// <summary>
        /// Maps collection of <see cref="SnippetOverviewDto"/> to collection of <see cref="SnippetOverviewDto"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <returns>Paged response.</returns>
        public static PagedResponse<SnippetOverviewDto> ToPagedResponse(this PagedEntities<SnippetOverviewDto> entities)
        {
            return Mapper.Map<PagedResponse<SnippetOverviewDto>>(entities);
        }
    }
}
