using ZODs.Api.Repository.Entities.Enums;
using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos;

public sealed class AILensInputDto
{
    [NotEmptyGuid]
    public Guid? Id { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public string Name { get; set; } = null!;

    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string BehaviorInstruction { get; set; } = null!;

    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string ResponseInstruction { get; set; } = null!;

    [MaxLength(100)]
    public string? Tooltip { get; set; }

    [Required]
    public AILensTargetKind TargetKind { get; set; }

    public Guid? UserId { get; set; }

    public Guid? WorkspaceId { get; set; }
}