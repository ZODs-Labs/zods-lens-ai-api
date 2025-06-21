using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.Managers;
using ZODs.Api.Repository.Repositories.Interfaces;
using ZODs.Common.Extensions;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Repository.Dtos;

namespace ZODs.Api.Service
{
    public sealed class UserInfoService(
        IUnitOfWork<ZodsContext> unitOfWork,
        IFeatureLimitationSyncManager featureLimitationSyncManager,
        ICacheService cacheService) : IUserInfoService
    {
        private readonly IUnitOfWork<ZodsContext> unitOfWork = unitOfWork;
        private readonly IFeatureLimitationSyncManager featureLimitationSyncManager = featureLimitationSyncManager;
        private readonly ICacheService cacheService = cacheService;

        private IUsersRepository UsersRepository => this.unitOfWork.GetRepository<IUsersRepository>();
        private IPricingPlanRepository PricingPlanRepository => this.unitOfWork.GetRepository<IPricingPlanRepository>();

        public async Task<UserInfoDto> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await this.UsersRepository.FirstOrDefaultAsync(
                x => x.Id == userId,
                selector: x => new
                {
                    x.Email,
                    x.RegistrationType,
                },
                cancellationToken: cancellationToken).NoSync();
            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {userId} not found.");
            }

            var pricingPlanDto = await this.PricingPlanRepository.GetUserPricingPlanInfo(userId, cancellationToken).NoSync();
            var features = new List<string>();
            var limitations = new Dictionary<string, object>();

            if (pricingPlanDto != null)
            {
                // Get all pricing plan features
                var pricingPlanFeatures = await PricingPlanRepository.GetPricingPlanFeatures(pricingPlanDto.PricingPlanId, cancellationToken).NoSync();
                features = pricingPlanFeatures.Select(x => x.Key).ToList();

                await featureLimitationSyncManager.SyncAllLimitationsAsync(userId, pricingPlanDto.PricingPlanId, cancellationToken).NoSync();

                var userFeatureLimitationKey = CacheKeyGenerator.GetUserFeatureLimitationsUsageKey(userId);
                limitations = await cacheService.Get<Dictionary<string, object>>(userFeatureLimitationKey, cancellationToken);
            }

            // Get all direct user features
            var userFeatures = await this.UsersRepository.GetUserFeaturesAsync(userId, cancellationToken).NoSync();
            var userFeatureKeys = userFeatures.Select(x => x.Key).ToArray();

            features.AddRange(userFeatureKeys);

            var userInfoDto = new UserInfoDto(
                email: user.Email ?? string.Empty,
                pricingPlanDto?.PricingPlanType,
                pricingPlanDto?.HasValidSubscription ?? false,
                isPaidPlan: pricingPlanDto?.IsPaidPlan == true,
                features,
                limitations);

            return userInfoDto;
        }

        public async Task<UserInfoDto> GetUserInfoCachedAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userInfoCacheKey = CacheKeyGenerator.GetUserInfoKey(userId);
            var userInfoDto = await this.cacheService.Get<UserInfoDto>(userInfoCacheKey, cancellationToken).NoSync();
            if (userInfoDto == null)
            {
                userInfoDto = await this.GetUserInfoAsync(userId, cancellationToken).NoSync();
                await this.cacheService.Set(userInfoCacheKey, userInfoDto, options: null, cancellationToken).NoSync();
            }

            var userFeatureLimitationKey = CacheKeyGenerator.GetUserFeatureLimitationsUsageKey(userId);
            var limitations = await cacheService.Get<Dictionary<string, object>>(userFeatureLimitationKey, cancellationToken);
            if (limitations != null)
            {
                userInfoDto.Limitations = limitations;
            }

            return userInfoDto;
        }

        public async Task<ICollection<string>> GetUserFeaturesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var userInfoKey = CacheKeyGenerator.GetUserInfoKey(userId);
            var userInfo = await this.cacheService.Get<UserInfoDto>(userInfoKey, cancellationToken).NoSync();

            if (userInfo == null)
            {
                userInfo = await this.GetUserInfoAsync(userId, cancellationToken).NoSync();
                await this.cacheService.Set(userInfoKey, userInfo, options: null, cancellationToken).NoSync();
            }

            return userInfo.Features.ToArray();
        }

        public async Task SyncUserInfoCacheAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userInfoDto = await this.GetUserInfoAsync(userId, cancellationToken).NoSync();

            var userInfoCacheKey = CacheKeyGenerator.GetUserInfoKey(userId);
            await this.cacheService.Set(userInfoCacheKey, userInfoDto, options: null, cancellationToken).NoSync();
        }

        public async Task<UserPricingPlanDto> GetUserPricingPlanAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeyGenerator.GetUserPricingPlanIdKey(userId);
            var userPricingPlan = await this.cacheService.Get<UserPricingPlanDto>(cacheKey, cancellationToken);
            if (userPricingPlan == null)
            {
                userPricingPlan = await this.PricingPlanRepository.GetUserPricingPlanInfo(userId, cancellationToken);

                await this.cacheService.Set(cacheKey, userPricingPlan, options: null, cancellationToken);
            }

            if (userPricingPlan == null)
            {
                throw new KeyNotFoundException($"User pricing plan not found for user {userId}");
            }

            return userPricingPlan;
        }
    }
}