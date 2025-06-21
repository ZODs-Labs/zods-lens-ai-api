using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.Mappers.Profiles;

namespace ZODs.Api.Service.Mappers
{
    public static class WorkspaceMapper
    {
        static WorkspaceMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<WorkspaceMapperProfile>()).CreateMapper();
        }

        public static IMapper Mapper { get; }

        /// <summary>
        /// Maps <see cref="Workspace"/> to <see cref="WorkspaceDto"/>.
        /// </summary>
        /// <param name="Workspace">Entity to map from.</param>
        /// <returns>Mapped <see cref="WorkspaceDto"/>.</returns>
        public static WorkspaceDto ToDto(this Workspace Workspace)
        {
            return Mapper.Map<WorkspaceDto>(Workspace);
        }

        /// <summary>
        /// Maps collection of <see cref="Workspace"/> to collection of <see cref="WorkspaceDto"/>.
        /// </summary>
        /// <param name="entities">Collection of entities to map from.</param>
        /// <returns>Collection of mapped <see cref="WorkspaceDto"/>.</returns>
        public static IEnumerable<WorkspaceDto> ToDto(this IEnumerable<Workspace> entities)
        {
            return Mapper.Map<IEnumerable<WorkspaceDto>>(entities);
        }

        /// <summary>
        /// Maps <see cref="WorkspaceDto"/> to <see cref="Workspace"/>.
        /// </summary>
        /// <param name="WorkspaceDto">Dto to map from.</param>
        /// <returns>Mapped entity.</returns>
        public static Workspace ToEntity(this WorkspaceDto WorkspaceDto)
        {
            return Mapper.Map<Workspace>(WorkspaceDto);
        }

        /// <summary>
        /// Maps collection of <see cref="WorkspaceDto"/> to collection of <see cref="Workspace"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <returns>Paged response.</returns>
        public static PagedResponse<WorkspaceDto> ToDto(this PagedEntities<Workspace> entities)
        {
            return Mapper.Map<PagedResponse<WorkspaceDto>>(entities);
        }

        /// <summary>
        /// Maps paged entities collection of <see cref="WorkspaceMemberDto"/> to paged response collection of <see cref="WorkspaceMemberDto"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <returns>Paged response.</returns>
        public static PagedResponse<WorkspaceMemberDto> ToDto(this PagedEntities<WorkspaceMemberDto> entities)
        {
            return Mapper.Map<PagedResponse<WorkspaceMemberDto>>(entities);
        }

        /// <summary>
        /// Maps paged entities collection of <see cref="UserWorkspaceDto"/> to paged response collection of <see cref="UserWorkspaceDto"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <returns>Paged response.</returns>
        public static PagedResponse<UserWorkspaceDto> ToDto(this PagedEntities<UserWorkspaceDto> entities)
        {
            return Mapper.Map<PagedResponse<UserWorkspaceDto>>(entities);
        }

        /// <summary>
        /// Maps paged entities collection of <see cref="WorkspaceInviteMemberDto"/> to paged response collection of <see cref="WorkspaceInviteMemberDto"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <returns>Paged response.</returns>
        public static PagedResponse<WorkspaceInviteMemberDto> ToDto(this PagedEntities<WorkspaceInviteMemberDto> entities)
        {
            return Mapper.Map<PagedResponse<WorkspaceInviteMemberDto>>(entities);
        }
    }
}
