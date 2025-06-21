using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Common;
using ZODs.Api.Service.Factories.Interfaces;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;

namespace ZODs.Api.Service
{
    public sealed class FeatureLimitationService : IFeatureLimitationService
    {
        private readonly IFeatureLimitationStrategyFactory featureLimitationStrategyFactory;

        public FeatureLimitationService(IFeatureLimitationStrategyFactory featureLimitationStrategyFactory)
        {
            this.featureLimitationStrategyFactory = featureLimitationStrategyFactory;
        }

        public async Task<T> GetUserFeatureLimitationUsageAsync<T>(
            FeatureLimitationIndex limitationIndex,
            FeatureLimitationContext context,
            CancellationToken cancellationToken)
        {
            var strategy = (IFeatureLimitationStrategy<T>)this.featureLimitationStrategyFactory.Create(limitationIndex);
            var value = await strategy.GetUsageLeftCachedAsync(context, cancellationToken);

            if (value == null)
            {
                // Fallback - get usage left from DB
                value = await strategy.GetUsageLeftAsync(context, cancellationToken);
            }

            return value;
        }
    }
}