using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.Dtos;

public sealed class WorkspaceDto
{
    public Guid Id { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set; } = default!;

    [Required]
    [MinLength(2)]
    [MaxLength(500)]
    public string Description { get; set; } = default!; 

    public bool IsActive { get; set; }
}
