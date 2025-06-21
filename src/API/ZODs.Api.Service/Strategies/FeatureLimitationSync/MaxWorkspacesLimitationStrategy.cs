using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Interfaces;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public class MaxWorkspacesLimitationStrategy
        : BaseFeatureLimitationStrategy,
          IFeatureLimitationStrategy<int>
    {
        private readonly IWorkspacesRepository workspaceRepository;

        public MaxWorkspacesLimitationStrategy(
            IWorkspacesRepository workspaceRepository,
            ICacheService cache,
            IPricingPlanService pricingPlanService,
            IPricingPlanFeaturesRepository pricingPlanFeaturesRepository)
            : base(cache, pricingPlanFeaturesRepository, pricingPlanService)
        {
            this.workspaceRepository = workspaceRepository;

            this.limitationIndex = FeatureLimitationIndex.MaxWorkspaces;
            this.featureIndex = FeatureIndex.Workspaces;
        }

        public async Task<int> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            int currentWorkspacesCount = await workspaceRepository.GetUserCreatedWorkspacesCount(userId, cancellationToken);

            var limitationValue = await GetFeauterLimitationValue(userId, cancellationToken);
            int maxWorkspaces = string.IsNullOrWhiteSpace(limitationValue) ? 0 : int.Parse(limitationValue);

            int workspacesToCreateLeft = maxWorkspaces - currentWorkspacesCount;

            return workspacesToCreateLeft;
        }

        public async Task<int> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var workspacesToCreateLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);
            return workspacesToCreateLeft ?? -1;
        }

        public async Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var workspacesToCreateLeft = await GetLimitationUsageFromCacheAsync<int?>(userId, cancellationToken);
            workspacesToCreateLeft ??= await GetUsageLeftAsync(context, cancellationToken);

            return workspacesToCreateLeft <= 0;
        }

        public async Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var workspacesToCreateLeft = await GetUsageLeftAsync(context, cancellationToken);

            await SyncUserFeatureLimitationUsageAsync(
                     userId,
                     workspacesToCreateLeft,
                     cancellationToken);
        }
    }
}