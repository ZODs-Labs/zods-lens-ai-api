using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos
{
    public sealed class UserSubscriptionDto
    {
        public PricingPlanType PricingPlanType { get; set; }

        public PricingPlanVariantType PricingPlanVariantType { get; set; }

        public string? SubscriptionStatus { get; set; } = null!;

        public string SubscriptionStatusFormatted { get; set; } = null!;

        public string? UpdatePaymentUrl { get; set; } = null!;

        public DateTime? NextBillingDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}