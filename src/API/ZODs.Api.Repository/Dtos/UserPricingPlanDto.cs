using ZODs.Api.Repository.Entities.Enums;
using SubStatus = ZODs.Common.Constants.SubscriptionStatus;

namespace ZODs.Api.Repository.Dtos
{
    public sealed class UserPricingPlanDto
    {
        public Guid PricingPlanId { get; set; }

        public PricingPlanType PricingPlanType { get; set; }

        public string? SubscriptionStatus { get; set; } = null!;

        public UserRegistrationType RegistrationType { get; set; }

        public bool HasActivePricingPlan { get; set; }

        public bool IsPaidPlan { get; set; }

        public DateTime? EndDate { get; set; }

        public bool HasValidSubscription => PricingPlanType == PricingPlanType.Free || 
                                            SubscriptionStatus == SubStatus.Active ||
                                            SubscriptionStatus == SubStatus.PastDue ||
                                            (SubscriptionStatus == SubStatus.Cancelled && EndDate != null && EndDate > DateTime.UtcNow) ||
                                            (RegistrationType == UserRegistrationType.WorkspaceInvite && HasActivePricingPlan);
    }
}