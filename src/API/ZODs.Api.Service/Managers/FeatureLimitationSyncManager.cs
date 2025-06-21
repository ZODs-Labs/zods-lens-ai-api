using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Common;
using ZODs.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZODs.Api.Service.Factories.Interfaces;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Repositories.Interfaces;

namespace ZODs.Api.Service.Managers
{
    public class FeatureLimitationSyncManager(
        IBackgroundTaskQueue taskQueue,
        IFeatureLimitationStrategyFactory factory,
        ILogger<FeatureLimitationSyncManager> logger,
        IUnitOfWork<ZodsContext> unitOfWork) : IFeatureLimitationSyncManager
    {
        private readonly IBackgroundTaskQueue _taskQueue = taskQueue;
        private readonly IFeatureLimitationStrategyFactory factory = factory;
        private readonly IUnitOfWork<ZodsContext> unitOfWork = unitOfWork;
        private readonly ILogger<FeatureLimitationSyncManager> logger = logger;

        public void QueueLimitationUsageSync(Guid userId, FeatureLimitationIndex featureLimitationIndex)
        {
            _taskQueue.QueueBackgroundWorkItem(async (cancellationToken, serviceProvider) =>
            {
                using var scope = serviceProvider.CreateScope();

                var factory = scope.ServiceProvider.GetRequiredService<IFeatureLimitationStrategyFactory>();
                var scopeLogger = scope.ServiceProvider.GetRequiredService<ILogger<FeatureLimitationSyncManager>>();

                var strategy = factory.Create(featureLimitationIndex);
                await strategy.SyncAsync(FeatureLimitationContext.Create(userId), cancellationToken);

                scopeLogger.LogInformation(
                    "Synced feature limitation usage for user {userId} and feature {featureLimitationIndex}.",
                    userId,
                    featureLimitationIndex);
            });
        }

        public void QueueAllLimitationsSync(Guid userId)
        {
            _taskQueue.QueueBackgroundWorkItem(async (cancellationToken, serviceProvider) =>
            {
                using var scope = serviceProvider.CreateScope();

                var featureLimitationSyncManager = scope.ServiceProvider.GetRequiredService<IFeatureLimitationSyncManager>();
                var pricingPlanService = scope.ServiceProvider.GetRequiredService<IPricingPlanService>();
                var scopeLogger = scope.ServiceProvider.GetRequiredService<ILogger<FeatureLimitationSyncManager>>();

                var userPricingPlanId = await pricingPlanService.GetUserPricingPlanIdAsync(userId, cancellationToken);
                await featureLimitationSyncManager.SyncAllLimitationsAsync(userId, userPricingPlanId, cancellationToken);

                scopeLogger.LogInformation("Synced all feature limitation usages for user {userId}.", userId);
            });
        }

        public async Task SyncAllLimitationsAsync(Guid userId, Guid pricingPlanId, CancellationToken cancellationToken = default)
        {
            var allLimitationIndices = await unitOfWork.GetRepository<IPricingPlanRepository>()
                .GetPricingPlanFeatureLimitationIndexesAsync(pricingPlanId, cancellationToken);

            foreach (var limitationIndex in allLimitationIndices)
            {
                var strategy = factory.Create(limitationIndex);
                await strategy.SyncAsync(FeatureLimitationContext.Create(userId), cancellationToken).NoSync();
            }

            logger.LogInformation("Synced all feature limitation usages for user {userId}.", userId);
        }
    }
}