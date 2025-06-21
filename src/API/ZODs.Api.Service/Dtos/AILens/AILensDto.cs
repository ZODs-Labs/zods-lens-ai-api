using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Service.Dtos;

public sealed class AILensDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string BehaviorInstruction { get; set; } = null!;

    public string ResponseInstruction { get; set; } = null!;

    public string? Tooltip { get; set; }

    public AILensTargetKind TargetKind { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsBuiltIn { get; set; }

    public Guid? UserId { get; set; }

    public Guid? WorkspaceId { get; set; }
}