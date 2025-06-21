using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;
using ZODs.Api.Service.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public sealed class MaxWorkspaceSnippetsLimitationStrategy
        : BaseFeatureLimitationStrategy,
          IFeatureLimitationStrategy<Dictionary<Guid, int>>
    {
        private readonly IWorkspacesRepository workspaceRepository;

        public MaxWorkspaceSnippetsLimitationStrategy(
            IWorkspacesRepository workspaceRepository,
            ICacheService cache,
            IPricingPlanFeaturesRepository pricingPlanFeaturesRepository,
            IPricingPlanService pricingPlanService)
            : base(cache, pricingPlanFeaturesRepository, pricingPlanService)
        {
            this.workspaceRepository = workspaceRepository;

            this.limitationIndex = FeatureLimitationIndex.MaxWorkspaceSnippets;
            this.featureIndex = FeatureIndex.Workspaces;
        }

        public async Task<Dictionary<Guid, int>> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var workspaceIdToSnippetsCountMap = await workspaceRepository.CountSnippetsByWorkspacesForUser(userId, cancellationToken);

            var limitationValue = await GetFeauterLimitationValue(userId, cancellationToken);
            int maxSnippetsPerWorkspace = string.IsNullOrWhiteSpace(limitationValue) ? 0 : int.Parse(limitationValue);

            var snippetsPerWorkspceLeft = workspaceIdToSnippetsCountMap.ToDictionary(x => x.Key, x => maxSnippetsPerWorkspace - x.Value);

            return snippetsPerWorkspceLeft;
        }

        public async Task<Dictionary<Guid, int>> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetsPerWorkspceLeft = await GetLimitationUsageFromCacheAsync<Dictionary<Guid, int>?>(userId, cancellationToken);
            return snippetsPerWorkspceLeft ?? new Dictionary<Guid, int>();
        }

        public async Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetsPerWorkspceLeft = await GetLimitationUsageFromCacheAsync<Dictionary<Guid, int>?>(userId, cancellationToken);
            snippetsPerWorkspceLeft ??= await GetUsageLeftAsync(context, cancellationToken);

            var workspaceLimitation = snippetsPerWorkspceLeft.FirstOrDefault(x => x.Key == context.WorkspaceId);
            var hasReachedLimitation = workspaceLimitation.Value <= 0;

            return hasReachedLimitation;
        }
        public async Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var snippetsPerWorkspceLeft = await GetUsageLeftAsync(context, cancellationToken);

            await SyncUserFeatureLimitationUsageAsync(
                     userId,
                     snippetsPerWorkspceLeft,
                     cancellationToken);
        }
    }
}