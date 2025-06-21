using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos
{
    public sealed class PricingPlanFeatureDto
    {
        public string Key { get; set; } = null!;

        public FeatureIndex Index { get; set; }

        public IEnumerable<PricingPlanFeatureLimitationDto> Limitations { get; set; } = new List<PricingPlanFeatureLimitationDto>();
    }
}