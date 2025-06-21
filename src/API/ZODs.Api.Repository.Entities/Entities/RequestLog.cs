using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities;

public sealed class RequestLog : BaseEntity
{
    [Required]
    public string Route { get; set; } = null!;
 
    public string? Metadata { get; set; } = null!;

    public Guid? UserId { get; set; }
}
