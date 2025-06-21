using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;

namespace ZODs.Api.Service.Mappers.Profiles;

public class WorkspaceMapperProfile : Profile
{
    public WorkspaceMapperProfile()
    {
        this.CreateMap<Workspace, WorkspaceDto>().ReverseMap();
        this.CreateMap<PagedEntities<WorkspaceMemberDto>, PagedResponse<WorkspaceMemberDto>>();
        this.CreateMap<PagedEntities<UserWorkspaceDto>, PagedResponse<UserWorkspaceDto>>();
        this.CreateMap<PagedEntities<Workspace>, PagedResponse<WorkspaceDto>>();
        
        // Workspace member invites
        this.CreateMap<PagedEntities<WorkspaceInviteMemberDto>, PagedResponse<WorkspaceInviteMemberDto>>();
    }
}
