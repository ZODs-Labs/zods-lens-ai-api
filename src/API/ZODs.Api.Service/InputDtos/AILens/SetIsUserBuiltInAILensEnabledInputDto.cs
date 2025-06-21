using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos.AILens;

public sealed class SetIsUserBuiltInAILensEnabledInputDto
{
    [Required]
    public bool IsEnabled { get; set; }
}