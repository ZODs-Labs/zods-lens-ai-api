using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZODs.Api.Repository.Entities.AI;

public class UserAICreditBalance : BaseEntity
{
    [Required]
    [Range(0, 100_000)]
    public int Gpt3Credits { get; set; }

    [Required]
    [Range(0, 100_000)]
    public int Gpt4Credits { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}