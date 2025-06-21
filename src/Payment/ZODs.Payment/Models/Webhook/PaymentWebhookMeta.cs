using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Webhook
{
    public sealed class PaymentWebhookMeta
    {
        [JsonPropertyName("test_mode")]
        public bool IsTestMode { get; set; }

        [JsonPropertyName("event_name")]
        public string? EventName { get; set; }

        [JsonPropertyName("custom_data")]
        public ICollection<string> CustomData { get; set; } = new List<string>();
    }
}