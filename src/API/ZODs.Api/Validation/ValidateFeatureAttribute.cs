
using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Validation
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ValidateFeatureAttribute(
        string feature,
        FeatureLimitationIndex[] limitationIndexes = null,
        PricingPlanType[] pricingPlanTypes = null,
        bool requiredPaidPlan = false,
        string errorMessage = "") : Attribute
    {
        public string Feature { get; } = feature;

        public FeatureLimitationIndex[] LimitationIndexes { get; } = limitationIndexes ?? [];

        public PricingPlanType[] PricingPlanTypes { get; } = pricingPlanTypes ?? [];

        public bool RequirePaidPlan { get; } = requiredPaidPlan;

        public string ErrorMessage { get; } = errorMessage;
    }
}