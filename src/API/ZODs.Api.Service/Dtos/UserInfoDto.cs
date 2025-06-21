using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Service.Dtos
{
    public sealed class UserInfoDto(
        string email,
        PricingPlanType? pricingPlanType,
        bool hasValidSubscription,
        bool isPaidPlan,
        IEnumerable<string> features,
        IDictionary<string, object> limitations)
    {
        public string Email { get; set; } = email;

        public PricingPlanType? PricingPlanType { get; set; } = pricingPlanType;

        public bool IsSubscribed => PricingPlanType.HasValue;

        /// <summary>
        /// Valid subscription means that the subscription is active, past due or cancelled with a future end date.
        /// </summary>
        public bool HasValidSubscription { get; set; } = hasValidSubscription;

        public bool IsPaidPlan { get; set; } = isPaidPlan;

        public IEnumerable<string> Features { get; set; } = features;

        public IDictionary<string, object> Limitations { get; set; } = limitations;
    }
}