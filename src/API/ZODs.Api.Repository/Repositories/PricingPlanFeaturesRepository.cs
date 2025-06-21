using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Repository
{
    public sealed class PricingPlanFeaturesRepository : Repository<PricingPlanFeature, ZodsContext>, IPricingPlanFeaturesRepository
    {
        public PricingPlanFeaturesRepository(ZodsContext context)
            : base(context)
        {
        }

        public async Task<ICollection<PricingPlanFeatureLimitation>> GetPricingPlanFeatureLimitations(
            Guid pricingPlanId,
            FeatureIndex featureIndex,
            CancellationToken cancellationToken = default)
        {
            var limitations = await Context.PricingPlanFeatureLimitations
                .AsNoTracking()
                .Where(x => x.PricingPlanFeature.PricingPlanId == pricingPlanId && 
                            x.FeatureLimitation.Feature.FeatureIndex == featureIndex)
                .ToListAsync(cancellationToken);

            return limitations;
        }

        public async Task<PricingPlanFeatureLimitation?> GetPricingPlanFeatureLimitation(
            Guid pricingPlanId,
            FeatureIndex featureIndex,
            FeatureLimitationIndex pricingPlanFeatureLimitationIndex,
            CancellationToken cancellationToken = default)
        {
            var limitation = await Context.PricingPlanFeatureLimitations
                .AsNoTracking()
                .Where(x => x.PricingPlanFeature.PricingPlanId == pricingPlanId &&
                            x.FeatureLimitation.Feature.FeatureIndex == featureIndex &&
                            x.FeatureLimitation.Index == pricingPlanFeatureLimitationIndex)
                .FirstOrDefaultAsync(cancellationToken);

            return limitation;
        }

        public async Task<string> GetPricingPlanFeatureLimitationValue(
            Guid pricingPlanId,
            FeatureIndex featureIndex,
            FeatureLimitationIndex pricingPlanFeatureLimitationIndex,
            CancellationToken cancellationToken = default)
        {
            var limitationValue = await Context.PricingPlanFeatureLimitations
                .Where(x => x.PricingPlanFeature.PricingPlanId == pricingPlanId &&
                            x.FeatureLimitation.Feature.FeatureIndex == featureIndex &&
                            x.FeatureLimitation.Index == pricingPlanFeatureLimitationIndex)
                .Select(x => x.Value)
                .FirstOrDefaultAsync(cancellationToken);

            return limitationValue ?? string.Empty;
        }
    }
}