using System.Text.Json.Serialization;

namespace ZODs.Payment.Models
{
    public class PaymentApiPayload<TData>
    {
        [JsonPropertyName("data")]
        public TData Data { get; set; } = default!;

        [JsonPropertyName("links")]
        public Dictionary<string, string> Links { get; set; } = new();
    }
}