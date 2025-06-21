using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Common;
using Newtonsoft.Json;
using ZODs.Api.Service.Dtos;

namespace ZODs.Api.Service.Strategies.FeatureLimitationSync
{
    public abstract class BaseFeatureLimitationStrategy(
        ICacheService cache,
        IPricingPlanFeaturesRepository pricingPlanFeaturesRepository,
        IPricingPlanService pricingPlanService)
    {
        protected readonly ICacheService cache = cache;
        protected readonly IPricingPlanFeaturesRepository pricingPlanFeaturesRepository = pricingPlanFeaturesRepository;
        protected readonly IPricingPlanService pricingPlanService = pricingPlanService;
        protected FeatureLimitationIndex limitationIndex;
        protected FeatureIndex featureIndex;

        protected async Task<string> GetFeauterLimitationValue(Guid userId, CancellationToken cancellationToken = default)
        {
            var userPricingPlanId = await pricingPlanService.GetUserPricingPlanIdAsync(userId, cancellationToken);
            var limitationValue = await pricingPlanFeaturesRepository.GetPricingPlanFeatureLimitationValue(
                                                       userPricingPlanId,
                                                       featureIndex,
                                                       limitationIndex,
                                                       cancellationToken);

            return limitationValue;
        }

        protected async Task SyncUserFeatureLimitationUsageAsync(
            Guid userId,
            object value,
            CancellationToken cancellationToken = default)
        {
            var usageKey = FeatureLimitationIndexToUsageKeyMapper.GetUsageKey(limitationIndex);
            await SyncUserFeatureLimitationUsageAsync(userId, usageKey, value, cancellationToken);
        }

        protected async Task SyncUserFeatureLimitationUsageAsync(
            Guid userId,
            string usageKey,
            object value,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeyGenerator.GetUserFeatureLimitationsUsageKey(userId);

            var userFeatureLimitationsUsage = await cache.Get<Dictionary<string, object>>(cacheKey, cancellationToken);
            if (userFeatureLimitationsUsage == null)
            {
                userFeatureLimitationsUsage = new Dictionary<string, object>
                {
                    { usageKey, value }
                };
            }
            else
            {
                if (!userFeatureLimitationsUsage.TryAdd(usageKey, value))
                {
                    userFeatureLimitationsUsage[usageKey] = value;
                }
            }

            await cache.Set(cacheKey, userFeatureLimitationsUsage, cancellationToken: cancellationToken);

            // Important step to sync user info limitations
            await SyncUserInfoLimitationsAsync(userId, userFeatureLimitationsUsage, cancellationToken);
        }

        protected async Task<T?> GetLimitationUsageFromCacheAsync<T>(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeyGenerator.GetUserFeatureLimitationsUsageKey(userId);
            var userFeatureLimitationsUsage = await cache.Get<Dictionary<string, object>>(cacheKey, cancellationToken);
            if (userFeatureLimitationsUsage == null)
            {
                return default;
            }

            var usageKey = FeatureLimitationIndexToUsageKeyMapper.GetUsageKey(limitationIndex);
            if (!userFeatureLimitationsUsage.ContainsKey(usageKey))
            {
                return default;
            }

            var value = userFeatureLimitationsUsage[usageKey]?.ToString() ?? string.Empty;

            return JsonConvert.DeserializeObject<T>(value);
        }

        private async Task SyncUserInfoLimitationsAsync(
            Guid userId,
            IDictionary<string, object> limitations,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeyGenerator.GetUserInfoKey(userId);
            var userInfoDto = await cache.Get<UserInfoDto>(cacheKey, cancellationToken);
            if (userInfoDto == null)
            {
                return;
            }

            userInfoDto.Limitations = limitations;
            await cache.Set(cacheKey, userInfoDto, cancellationToken: cancellationToken);
        }
    }
}