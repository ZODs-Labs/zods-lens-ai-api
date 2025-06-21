using ZODs.Api.Repository.Entities.Enums;
using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos.Workspace
{
    public sealed class InviteWorkspaceMemberInputDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [ValidEnum]
        public WorkspaceMemberRoleIndex RoleIndex { get; set; }
    }
}