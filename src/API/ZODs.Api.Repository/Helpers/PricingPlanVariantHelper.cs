using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Helpers;

public static class PricingPlanVariantHelper
{
    public sealed record PricingPlanVariantKey(PricingPlanType PlanType, PricingPlanVariantType VariantType);

    private static readonly Dictionary<PricingPlanVariantKey, int>
        PricingPlanVariantIdsMap_PROD = new()
        {
            // Free
            { new PricingPlanVariantKey(PricingPlanType.Free, PricingPlanVariantType.Free), 00000 },

            // Basic
            { new PricingPlanVariantKey(PricingPlanType.Basic, PricingPlanVariantType.Monthly), 138432 },
            { new PricingPlanVariantKey(PricingPlanType.Basic, PricingPlanVariantType.Yearly),  138433},

            // Standard
            { new PricingPlanVariantKey(PricingPlanType.Standard, PricingPlanVariantType.Monthly), 138438 },
            { new PricingPlanVariantKey(PricingPlanType.Standard, PricingPlanVariantType.Yearly),  138439 },

            // Premium
            { new PricingPlanVariantKey(PricingPlanType.Premium, PricingPlanVariantType.Monthly), 138435 },
            { new PricingPlanVariantKey(PricingPlanType.Premium, PricingPlanVariantType.Yearly),  138436 },
        };

    private static readonly Dictionary<PricingPlanVariantKey, int>
        PricingPlanVariantIdsMap_DEV = new()
        {
            // Free
            { new PricingPlanVariantKey(PricingPlanType.Free, PricingPlanVariantType.Free), 00000 },

            // Basic
            { new PricingPlanVariantKey(PricingPlanType.Basic, PricingPlanVariantType.Monthly), 131625 },
            { new PricingPlanVariantKey(PricingPlanType.Basic, PricingPlanVariantType.Yearly),  131630 },

            // Standard
            { new PricingPlanVariantKey(PricingPlanType.Standard, PricingPlanVariantType.Monthly), 131633 },
            { new PricingPlanVariantKey(PricingPlanType.Standard, PricingPlanVariantType.Yearly),  131634},

            // Premium
            { new PricingPlanVariantKey(PricingPlanType.Premium, PricingPlanVariantType.Monthly), 131636 },
            { new PricingPlanVariantKey(PricingPlanType.Premium, PricingPlanVariantType.Yearly),  131637 },
        };

    public static int GetPricingPlanVariantId(PricingPlanType planType, PricingPlanVariantType variantType, bool isProduction)
    {
        var pricingPlanVariantKey = new PricingPlanVariantKey(planType, variantType);

        if (isProduction)
        {
            return PricingPlanVariantIdsMap_PROD[pricingPlanVariantKey];
        }

        return PricingPlanVariantIdsMap_DEV[pricingPlanVariantKey];
    }
}