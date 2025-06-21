using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Service.Services;

public sealed class WorkspaceDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public WorkspaceMemberRoleIndex UserRoleIndex { get; set; }
}

