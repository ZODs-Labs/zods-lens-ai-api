using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities
{
    public sealed class PricingPlanFeatureLimitation
    {
        public Guid FeatureLimitationId { get; set; }
        public FeatureLimitation FeatureLimitation { get; set; } = null!;

        public Guid PricingPlanFeatureId { get; set; }
        public PricingPlanFeature PricingPlanFeature { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Value { get; set; } = null!;
    }
}