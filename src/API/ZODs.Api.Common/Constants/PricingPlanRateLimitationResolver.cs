using ZODs.Api.Common.Enums;
using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Common.Constants;

public static class PricingPlanRateLimitationResolver
{
    private static readonly Dictionary<PricingPlanType, (int Minutes, int MaxRequests)> Mixtral8x7bPricingPlanRateLimitations = new()
    {
        { PricingPlanType.Free, (60, 5) },
        { PricingPlanType.Basic, (60, 10) },
        { PricingPlanType.Standard, (60, 20) },
        { PricingPlanType.Premium, (60, 25) },
    };

    private static readonly Dictionary<RateLimitationType, Dictionary<PricingPlanType, (int Minutes, int MaxRequests)>> RateLimitationTypeLimitations = new()
    {
        { RateLimitationType.Mixtral8x7b, Mixtral8x7bPricingPlanRateLimitations },
    };

    private static readonly Dictionary<RateLimitationType, string> RateLimitationMessages = new()
    {
        { RateLimitationType.Mixtral8x7b, "You've reached the current usage cap for Mistral 8x7B model. Try again in {0}. If you want to increase the usage cap, please [upgrade your pricing plan](https://app.zods.pro/plan)." },
    };

    public static (int Minutes, int MaxRequests) GetRateLimitationInfo(PricingPlanType pricingPlanType, RateLimitationType limitationType)
    {
        if (RateLimitationTypeLimitations.TryGetValue(limitationType, out var rateLimitationTypeLimitations))
        {
            if (rateLimitationTypeLimitations.TryGetValue(pricingPlanType, out var rateLimitationInfo))
            {
                return rateLimitationInfo;
            }
        }

        throw new InvalidOperationException($"Rate limitation info for pricing plan type {pricingPlanType} and limitation type {limitationType} was not found.");
    }

    public static string GetRateLimitationMessage(RateLimitationType limitationType)
    {
        if (RateLimitationMessages.TryGetValue(limitationType, out var rateLimitationMessage))
        {
            return rateLimitationMessage;
        }

        throw new InvalidOperationException($"Rate limitation message for limitation type {limitationType} was not found.");
    }
}
