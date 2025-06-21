using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Checkout
{
    public sealed class PaymentCheckoutAttributes
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}