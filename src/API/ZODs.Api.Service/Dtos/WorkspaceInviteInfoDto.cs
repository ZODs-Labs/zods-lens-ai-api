namespace ZODs.Api.Service.Dtos;

public sealed class WorkspaceInviteInfoDto
{
    public string WorkspaceName { get; set; } = default!;

    public string InviteEmail { get; set; } = default!;
}

