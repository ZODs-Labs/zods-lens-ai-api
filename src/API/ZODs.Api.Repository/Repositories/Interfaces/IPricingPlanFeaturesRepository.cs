using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Interfaces;

namespace ZODs.Api.Repository
{
    public interface IPricingPlanFeaturesRepository : IRepository<PricingPlanFeature>
    {
        Task<PricingPlanFeatureLimitation?> GetPricingPlanFeatureLimitation(
            Guid pricingPlanId,
            FeatureIndex featureIndex,
            FeatureLimitationIndex pricingPlanFeatureLimitationIndex,
            CancellationToken cancellationToken = default);

        Task<ICollection<PricingPlanFeatureLimitation>> GetPricingPlanFeatureLimitations(
            Guid pricingPlanId,
            FeatureIndex featureIndex,
            CancellationToken cancellationToken = default);

        Task<string> GetPricingPlanFeatureLimitationValue(
            Guid pricingPlanId,
            FeatureIndex featureIndex,
            FeatureLimitationIndex pricingPlanFeatureLimitationIndex,
            CancellationToken cancellationToken = default);
    }
}