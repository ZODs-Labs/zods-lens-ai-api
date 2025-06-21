
using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos
{
    public sealed class UserWorkspaceDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public WorkspaceMemberRoleIndex? RoleIndex { get; set; }

        public bool IsOwner { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}