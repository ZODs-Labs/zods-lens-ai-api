using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Subscription
{
    public sealed class FirstSubscriptionItem
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("subscription_id")]
        public string? SubscriptionId { get; set; }
    }
}