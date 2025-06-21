using System.Text.Json.Serialization;

namespace ZODs.Payment.Models;

public class PaymentData<TAttributes>
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("attributes")]
    public TAttributes Attributes { get; set; } = default!;

    [JsonPropertyName("links")]
    public Dictionary<string, string> Links { get; set; } = new();
}