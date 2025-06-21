using ZODs.Api.Service.InputDtos.PricingPlan;
using ZODs.Payment.Models.Webhook;

namespace ZODs.Api.Service.Extensions.InputMapping
{
    public static class PaymentSubscriptionInputMappingExtensions
    {
        public static UpsertUserPricingPlanInputDto ToUpsertUserPricingPlanInputDto(
            this PaymentSubscriptionWebhookPayload subscriptionWebhookPayload)
        {
            var userId = subscriptionWebhookPayload.Meta.CustomData.FirstOrDefault();
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                throw new InvalidCastException("User id is not a valid guid.");
            }

            var subscription = subscriptionWebhookPayload.Data.Attributes;

            return new UpsertUserPricingPlanInputDto(
                userIdGuid,
                subscriptionId: subscriptionWebhookPayload.Data.Id,
                subscription.Status ?? string.Empty,
                subscription.StatusFormatted ?? string.Empty,
                subscription.VariantId ?? 0,
                subscription.CustomerId ?? 0,
                subscription.RenewsAt,
                subscription.EndsAt);
        }
    }
}