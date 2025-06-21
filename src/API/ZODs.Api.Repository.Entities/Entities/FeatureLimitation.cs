using ZODs.Api.Repository.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities
{
    public sealed class FeatureLimitation: BaseEntity
    {
        [Required]
        [MinLength(2)]
        [MaxLength(200)]    
        public string Name { get; set; } = null!;

        [Required]
        [MinLength(2)]
        [MaxLength(150)]
        public string Key { get; set; } = null!;

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Description { get; set; } = null!;

        [Required]
        public FeatureLimitationIndex Index { get; set; }

        public Guid FeatureId { get; set; }
        public Feature Feature { get; set; } = null!;
    }
}