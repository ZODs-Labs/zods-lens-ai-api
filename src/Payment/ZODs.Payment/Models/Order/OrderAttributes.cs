using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Order;

public class OrderAttributes
{
    [JsonPropertyName("urls")]
    public OrderUrls? Urls { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("refunded")]
    public bool Refunded { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("user_email")]
    public string UserEmail { get; set; } = default!;

    [JsonPropertyName("first_order_item")]
    public OrderItem FirstOrderItem { get; set; } = default!;

    [JsonPropertyName("discount_total_usd")]
    public int DiscountTotalUsd { get; set; }
}
