using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos
{
    public sealed class PricingPlanFeatureLimitationDto
    {
        public string Key { get; set; } = null!;

        public FeatureLimitationIndex Index { get; set; }

        public object Value { get; set; } = null!;
    }
}