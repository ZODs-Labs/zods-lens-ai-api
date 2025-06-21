using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Common;

namespace ZODs.Api.Service
{
    public interface IFeatureLimitationService
    {
        Task<T> GetUserFeatureLimitationUsageAsync<T>(FeatureLimitationIndex limitationIndex, FeatureLimitationContext context, CancellationToken cancellationToken);
    }
}