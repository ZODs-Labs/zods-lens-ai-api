namespace ZODs.Api.Service.InputDtos.PricingPlan
{
    public sealed class UpsertUserPricingPlanInputDto
    {
        public Guid UserId { get; set; }

        public string SubscriptionId { get; set; } = null!;

        public string SubscriptionStatus { get; set; } = null!;

        public string SubscriptionStatusFormatted { get; set; } = null!;

        public int VariantId { get; set; }

        public int CustomerId { get; set; }

        public DateTime? NextBillingDate { get; set; }

        public DateTime? EndDate { get; set; }

        public UpsertUserPricingPlanInputDto(
            Guid userId,
            string subscriptionId,
            string subscriptionStatus,
            string subscriptionStatusFormatted,
            int variantId,
            int customerId,
            DateTime? nextBillingDate,
            DateTime? endDate)
        {
            UserId = userId;
            SubscriptionId = subscriptionId;
            SubscriptionStatus = subscriptionStatus;
            SubscriptionStatusFormatted = subscriptionStatusFormatted;
            VariantId = variantId;
            CustomerId = customerId;
            NextBillingDate = nextBillingDate;
            EndDate = endDate;
        }
    }
}