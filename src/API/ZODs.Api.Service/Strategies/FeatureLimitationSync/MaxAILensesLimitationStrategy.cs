using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Interfaces;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public sealed class MaxAILensesLimitationStrategy
        : BaseFeatureLimitationStrategy,
        IFeatureLimitationStrategy<int>
    {
        private readonly IAILensRepository aILensRepository;

        public MaxAILensesLimitationStrategy(
            ICacheService cache,
            IPricingPlanFeaturesRepository pricingPlanFeaturesRepository,
            IPricingPlanService pricingPlanService,
            IAILensRepository aILensRepository)
            : base(cache, pricingPlanFeaturesRepository, pricingPlanService)
        {
            this.aILensRepository = aILensRepository;
            this.limitationIndex = FeatureLimitationIndex.MaxAILenses;
            this.featureIndex = FeatureIndex.AILens;
        }

        public async Task<int> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            int aiLensesCount = await aILensRepository.GetUserOwnAILensesCountAsync(userId, cancellationToken);

            var limitationValue = await GetFeauterLimitationValue(userId, cancellationToken);
            int maxAILenses = string.IsNullOrWhiteSpace(limitationValue) ?
               0 :
               int.Parse(limitationValue);

            int aiLensesToCreateLeft = maxAILenses - aiLensesCount;

            return aiLensesToCreateLeft;
        }

        public async Task<int> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var aiLensesLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);

            return aiLensesLeft ?? -1;
        }

        public async Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var aiLensesLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);
            aiLensesLeft ??= await GetUsageLeftAsync(context, cancellationToken);

            return aiLensesLeft <= 0;
        }

        public async Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var aiLensesLeft = await GetUsageLeftAsync(context, cancellationToken);

            await SyncUserFeatureLimitationUsageAsync(
                userId, 
                aiLensesLeft, 
                cancellationToken);
        }
    }
}