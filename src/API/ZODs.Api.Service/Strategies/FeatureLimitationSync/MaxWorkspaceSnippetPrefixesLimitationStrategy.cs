using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;
using ZODs.Api.Service.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public sealed class MaxWorkspaceSnippetPrefixesLimitationStrategy
        : BaseFeatureLimitationStrategy,
          IFeatureLimitationStrategy<Dictionary<Guid, int>>
    {
        private readonly ISnippetTriggerPrefixesRepository snippetTriggerPrefixesRepository;

        public MaxWorkspaceSnippetPrefixesLimitationStrategy(
            ICacheService cache,
            IPricingPlanService pricingPlanService,
            IPricingPlanFeaturesRepository pricingPlanFeaturesRepository,
            ISnippetTriggerPrefixesRepository snippetTriggerPrefixesRepository)
            : base(cache, pricingPlanFeaturesRepository, pricingPlanService)
        {
            this.snippetTriggerPrefixesRepository = snippetTriggerPrefixesRepository;

            this.limitationIndex = FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes;
            this.featureIndex = FeatureIndex.WorkspaceSnippetPrefixes;
        }

        public async Task<Dictionary<Guid, int>> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetPrefixesByWorkspace = await snippetTriggerPrefixesRepository.CountSnippetPrefixesByWorkspaceAsync(userId, cancellationToken);

            var limitationValue = await GetFeauterLimitationValue(userId, cancellationToken);
            int maxWorkspaceSnippetTriggerPrefixes = string.IsNullOrWhiteSpace(limitationValue) ? 0 : int.Parse(limitationValue);

            var snippetPrefixesByWorkspaceLeft = snippetPrefixesByWorkspace.ToDictionary(x => x.Key, x => maxWorkspaceSnippetTriggerPrefixes - x.Value);

            return snippetPrefixesByWorkspaceLeft;
        }

        public async Task<Dictionary<Guid, int>> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetPrefixesByWorkspaceLeft = await GetLimitationUsageFromCacheAsync<Dictionary<Guid, int>?>(userId, cancellationToken);
            return snippetPrefixesByWorkspaceLeft ?? new Dictionary<Guid, int>();
        }

        public async Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetPrefixesByWorkspaceLeft = await GetLimitationUsageFromCacheAsync<Dictionary<Guid, int>?>(userId, cancellationToken);
            snippetPrefixesByWorkspaceLeft ??= await GetUsageLeftAsync(context, cancellationToken);

            var workspaceLimitation = snippetPrefixesByWorkspaceLeft.FirstOrDefault(x => x.Key == context.WorkspaceId);
            var hasReachedLimitation = workspaceLimitation.Value <= 0;

            return hasReachedLimitation;
        }

        public async Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetPrefixesByWorkspaceLeft = await GetUsageLeftAsync(context, cancellationToken);

            await SyncUserFeatureLimitationUsageAsync(
                     userId,
                     snippetPrefixesByWorkspaceLeft,
                     cancellationToken);
        }
    }
}