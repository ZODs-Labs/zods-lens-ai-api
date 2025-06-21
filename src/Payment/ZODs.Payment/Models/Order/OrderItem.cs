using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Order;

public sealed class OrderItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("variant_id")]
    public int VariantId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
