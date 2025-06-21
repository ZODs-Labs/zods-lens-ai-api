namespace ZODs.Api.Repository.Entities
{
    public sealed class WorkspaceMember : BaseEntity
    {
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public Guid WorkspaceId { get; set; }

        public Workspace Workspace { get; set; } = null!;

        public ICollection<WorkspaceMemberRole> Roles { get; set; } = null!;
    }
}