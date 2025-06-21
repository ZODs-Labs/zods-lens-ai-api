using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities.Entities
{
    public sealed class Subscription : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Source { get; set; } = null!;

        [Required]
        public string EmailAddress { get; set; } = null!;
    }
}