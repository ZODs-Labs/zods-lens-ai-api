using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos;

public sealed class CheckAILensNameExistsInputDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public Guid? LensId { get; set; }
}