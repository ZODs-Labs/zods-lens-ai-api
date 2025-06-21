using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Interfaces;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public sealed class MaxPersonalSnippetsLimitationStrategy
        : BaseFeatureLimitationStrategy,
          IFeatureLimitationStrategy<int>
    {
        private readonly ISnippetsRepository snippetsRepository;

        public MaxPersonalSnippetsLimitationStrategy(
            ISnippetsRepository snippetsRepository,
            ICacheService cache,
            IPricingPlanService pricingPlanService,
            IPricingPlanFeaturesRepository pricingPlanFeaturesRepository)
            : base(cache, pricingPlanFeaturesRepository, pricingPlanService)
        {
            this.snippetsRepository = snippetsRepository;

            this.limitationIndex = FeatureLimitationIndex.MaxPersonalSnippets;
            this.featureIndex = FeatureIndex.PersonalSnippets;
        }

        public async Task<int> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            int personalSnippetsCount = await snippetsRepository.GetUserOwnSnippetsCountAsync(userId, cancellationToken);

            var limitationValue = await GetFeauterLimitationValue(userId, cancellationToken);
            int maxSnippets = string.IsNullOrWhiteSpace(limitationValue) ?
               0 :
               int.Parse(limitationValue);

            int workspacesToCreateLeft = maxSnippets - personalSnippetsCount;

            return workspacesToCreateLeft;
        }

        public async Task<int> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetsLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);
            return snippetsLeft ?? -1;
        }

        public async Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetsLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);
            snippetsLeft ??= await GetUsageLeftAsync(context, cancellationToken);

            return snippetsLeft <= 0;
        }

        public async Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetsLeft = await GetUsageLeftAsync(context, cancellationToken);

            await SyncUserFeatureLimitationUsageAsync(
                         userId,
                         snippetsLeft,
                         cancellationToken);
        }
    }
}