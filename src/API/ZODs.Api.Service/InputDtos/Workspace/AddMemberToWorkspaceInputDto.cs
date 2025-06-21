using ZODs.Api.Repository.Entities.Enums;
using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos.Workspace;

public sealed class AddMemberToWorkspaceInputDto
{
    public Guid WorkspaceId { get; set; }

    public Guid MemberUserId { get; set; }

    [Required]
    [ValidEnum]
    public WorkspaceMemberRoleIndex RoleIndex { get; set; }
}