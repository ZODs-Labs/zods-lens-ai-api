using ZODs.Api.Repository.Dtos;
using ZODs.Api.Service.Dtos;

namespace ZODs.Api.Service;

public interface IUserInfoService
{
    Task<ICollection<string>> GetUserFeaturesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserInfoDto> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserInfoDto> GetUserInfoCachedAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserPricingPlanDto> GetUserPricingPlanAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SyncUserInfoCacheAsync(Guid userId, CancellationToken cancellationToken);
}