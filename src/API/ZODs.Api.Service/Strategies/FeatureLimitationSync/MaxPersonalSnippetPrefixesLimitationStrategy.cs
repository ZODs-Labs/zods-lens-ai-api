using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;
using ZODs.Api.Service.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public sealed class MaxPersonalSnippetPrefixesLimitationStrategy
        : BaseFeatureLimitationStrategy,
          IFeatureLimitationStrategy<int>
    {
        private readonly ISnippetTriggerPrefixesRepository snippetTriggerPrefixesRepository;

        public MaxPersonalSnippetPrefixesLimitationStrategy(
            ICacheService cache,
            IPricingPlanService pricingPlanService,
            IPricingPlanFeaturesRepository pricingPlanFeaturesRepository,
            ISnippetTriggerPrefixesRepository snippetTriggerPrefixesRepository)
            : base(cache, pricingPlanFeaturesRepository, pricingPlanService)
        {
            this.snippetTriggerPrefixesRepository = snippetTriggerPrefixesRepository;

            this.limitationIndex = FeatureLimitationIndex.MaxPersonalSnippetPrefixes;
            this.featureIndex = FeatureIndex.PersonalSnippetPrefixes;
        }

        public async Task<int> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;

            // Get the current count of workspaces from database
            int totalPersonalSnippetTriggerPrefixes = await snippetTriggerPrefixesRepository.CountPersonalSnippetPrefixesAsync(userId, cancellationToken);

            var limitationValue = await GetFeauterLimitationValue(userId, cancellationToken);
            int maxPersonalSnippetTriggerPrefixes = string.IsNullOrWhiteSpace(limitationValue) ? 0 : int.Parse(limitationValue);

            // Calculate the remaining number of workspaces that the user can create
            int prefixesLeft = maxPersonalSnippetTriggerPrefixes - totalPersonalSnippetTriggerPrefixes;

            return prefixesLeft;
        }

        public async Task<int> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var prefixesLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);
            return prefixesLeft ?? -1;
        }

        public async Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var prefixesLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);
            prefixesLeft ??= await GetUsageLeftAsync(context, cancellationToken);

            return prefixesLeft <= 0;
        }

        public async Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var prefixesLeft = await GetUsageLeftAsync(context, cancellationToken);

            await SyncUserFeatureLimitationUsageAsync(
                     userId,
                     prefixesLeft,
                     cancellationToken);
        }
    }
}