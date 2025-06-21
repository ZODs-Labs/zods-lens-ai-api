using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Interfaces;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public sealed class MaxWorkspaceInvitesLimitationStrategy
        : BaseFeatureLimitationStrategy,
          IFeatureLimitationStrategy<Dictionary<Guid, int>>
    {
        private readonly IWorkspacesRepository workspaceRepository;

        public MaxWorkspaceInvitesLimitationStrategy(
            IWorkspacesRepository workspaceRepository,
            ICacheService cache,
            IPricingPlanService pricingPlanService,
            IPricingPlanFeaturesRepository pricingPlanFeaturesRepository)
            : base(cache, pricingPlanFeaturesRepository, pricingPlanService)
        {
            this.workspaceRepository = workspaceRepository;

            this.limitationIndex = FeatureLimitationIndex.MaxWorkspaceInvites;
            this.featureIndex = FeatureIndex.Workspaces;
        }

        public async Task<Dictionary<Guid, int>> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var workspaceIdToInvitesCountMap = await workspaceRepository.CountInvitesByWorksapcesForUser(userId, cancellationToken);

            var limitationValue = await GetFeauterLimitationValue(userId, cancellationToken);
            int maxInvitesPerWorkspace = string.IsNullOrWhiteSpace(limitationValue) ? 0 : int.Parse(limitationValue);

            var invitesPerWorkspaceLeft = workspaceIdToInvitesCountMap.ToDictionary(x => x.Key, x => maxInvitesPerWorkspace - x.Value);

            return invitesPerWorkspaceLeft;
        }

        public async Task<Dictionary<Guid, int>> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var invitesPerWorkspaceLeft = await GetLimitationUsageFromCacheAsync<Dictionary<Guid, int>?>(userId, cancellationToken);
            return invitesPerWorkspaceLeft ?? new Dictionary<Guid, int>();
        }

        public async Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var invitesPerWorkspaceLeft = await GetLimitationUsageFromCacheAsync<Dictionary<Guid, int>?>(userId, cancellationToken);
            invitesPerWorkspaceLeft ??= await GetUsageLeftAsync(context, cancellationToken);

            var workspaceLimitation = invitesPerWorkspaceLeft.FirstOrDefault(x => x.Key == context.WorkspaceId);
            var hasReachedLimitation = workspaceLimitation.Value <= 0;

            return hasReachedLimitation;
        }

        public async Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default)
        {
            var userId = context.UserId;
            var invitesPerWorkspaceLeft = await GetUsageLeftAsync(context, cancellationToken);

            await SyncUserFeatureLimitationUsageAsync(
                     userId,
                     invitesPerWorkspaceLeft, 
                     cancellationToken);
        }
    }
}