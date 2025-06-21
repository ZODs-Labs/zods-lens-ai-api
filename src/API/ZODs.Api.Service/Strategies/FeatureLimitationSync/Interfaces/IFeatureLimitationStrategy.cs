using ZODs.Api.Service.Interfaces;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces
{
    public interface IFeatureLimitationStrategy<T> : IFeatureLimitationBaseStrategy
    {
        Task<T> GetUsageLeftAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default);

        Task<T> GetUsageLeftCachedAsync(IFeatureLimitationContext context, CancellationToken cancellationToken = default);
    }
}