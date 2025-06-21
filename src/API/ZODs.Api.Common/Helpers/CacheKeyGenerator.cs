using ZODs.Api.Common.Enums;

namespace ZODs.Api.Common.Helpers;

public static class CacheKeyGenerator
{
    public static string BuiltInLenses => "BuiltInLenses";

    public static string GetUserInfoKey(Guid userId)
    {
        return $"UserInfo:{userId}";
    }

    public static string GetUserPricingPlanIdKey(Guid userId)
    {
        return $"UserPricingPlanId:{userId}";
    }

    public static string GetUserFeatureLimitationsUsageKey(Guid userId)
    {
        return $"UserFeatureLimitationsUsage:{userId}";
    }

    public static string GetPricingPlanFeaturesKey(Guid pricingPlanId)
    {
        return $"PricingPlanFeatures:{pricingPlanId}";
    }

    public static string GetAILensInstructionsKey(Guid lensId)
    {
        return $"AILensInstructions:{lensId}";
    }

    public static string GetAllUserAILensesInfoKey(Guid userId)
    {
        return $"AllUserAILensesInfo:{userId}";
    }

    public static string GetUserGpt3CreditsBalanceKey(Guid userId)
    {
        return $"UserAICreditsBalance:GPT3:{userId}";
    }

    public static string GetUserGpt4CreditsBalanceKey(Guid userId)
    {
        return $"UserAICreditsBalance:GPT4:{userId}";
    }

    public static string GetUserRateLimitationKey(RateLimitationType limitationType, Guid userId)
    {
        return $"UserRateLimitation:{limitationType}:{userId}";
    }
}