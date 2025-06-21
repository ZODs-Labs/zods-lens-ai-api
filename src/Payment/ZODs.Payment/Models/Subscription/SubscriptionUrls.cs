using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Subscription;

public sealed class SubscriptionUrls
{
    [JsonPropertyName("update_payment_method")]
    public string UpdatePaymentMethod { get; set; } = default!;

    [JsonPropertyName("customer_portal")]
    public string CustomerPortal { get; set; } = default!;
}