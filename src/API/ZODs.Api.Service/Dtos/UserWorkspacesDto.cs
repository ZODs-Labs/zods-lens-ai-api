using ZODs.Api.Repository.Dtos;

namespace ZODs.Api.Service.Dtos
{
    public sealed class UserWorkspacesDto
    {
        public IEnumerable<UserWorkspaceDto> OwnedWorkspaces { get; set; } = null!;

        public IEnumerable<UserWorkspaceDto> MemberWorkspaces { get; set; } = null!;
    }
}