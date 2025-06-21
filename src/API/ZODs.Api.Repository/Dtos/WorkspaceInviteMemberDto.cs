using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos
{
    public sealed class WorkspaceInviteMemberDto
    {
        public string Email { get; set; } = null!;

        public WorkspaceMemberRoleIndex RoleIndex { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsAccepted { get; set; }

        public string InvitedBy { get; set; } = null!;

        public DateTime InvitedAt { get; set; }
    }
}