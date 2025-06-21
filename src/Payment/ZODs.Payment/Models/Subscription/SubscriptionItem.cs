using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Subscription;

public sealed class SubscriptionItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("subscription_id")]
    public int SubscriptionId { get; set; }

    [JsonPropertyName("price_id")]
    public int PriceId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}