using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos
{
    public sealed class WorkspaceRoleDto
    {
        public string Name { get; set; } = null!;

        public WorkspaceMemberRoleIndex Index { get; set; }
    }
}