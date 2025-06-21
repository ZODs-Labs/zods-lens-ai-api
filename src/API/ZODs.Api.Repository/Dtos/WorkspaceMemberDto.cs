namespace ZODs.Api.Repository.Dtos
{
    public sealed class WorkspaceMemberDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EmailAddress { get; set; } = null!;

        public IEnumerable<WorkspaceRoleDto> WorkspaceRoles { get; set; } = new List<WorkspaceRoleDto>();
    }
}