using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Order;

public class OrderUrls
{
    [JsonPropertyName("receipt")]
    public string? Receipt { get; set; }
}
