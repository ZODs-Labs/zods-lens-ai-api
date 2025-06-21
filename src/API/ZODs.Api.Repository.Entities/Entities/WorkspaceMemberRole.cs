namespace ZODs.Api.Repository.Entities
{
    public sealed class WorkspaceMemberRole
    {
        public Guid WorkspaceMemberId { get; set; }

        public WorkspaceMember WorkspaceMember { get; set; } = null!;

        public Guid WorkspaceRoleId { get; set; }

        public WorkspaceRole Role { get; set; } = null!;
    }
}