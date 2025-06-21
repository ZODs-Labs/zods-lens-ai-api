using ZODs.Api.Service.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces
{
    public interface IFeatureLimitationBaseStrategy
    {
        Task<bool> HasReachedLimitationAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default);

        Task SyncAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default);
    }
}