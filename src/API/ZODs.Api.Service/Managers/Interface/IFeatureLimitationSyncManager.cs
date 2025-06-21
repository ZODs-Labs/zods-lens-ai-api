using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Service.Managers
{
    public interface IFeatureLimitationSyncManager
    {
        void QueueAllLimitationsSync(Guid userId);
        void QueueLimitationUsageSync(Guid userId, FeatureLimitationIndex featureLimitationIndex);
        Task SyncAllLimitationsAsync(Guid userId, Guid pricingPlanId, CancellationToken cancellationToken = default);
    }
}