using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Product
{
    public sealed class PaymentProductOptions
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; } = string.Empty;

        public PaymentProductOptions()
        {
        }
    }
}