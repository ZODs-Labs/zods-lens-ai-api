using System.Text.Json.Serialization;

namespace ZODs.Payment.InputModels.Checkout;

public sealed class PaymentCheckoutOptions
{
    [JsonPropertyName("embed")]
    public bool Embed { get; set; }

    [JsonPropertyName("media")]
    public bool Media { get; set; }

    [JsonPropertyName("logo")]
    public bool Logo { get; set; }

    [JsonPropertyName("desc")]
    public bool Desc { get; set; }

    [JsonPropertyName("discount")]
    public bool Discount { get; set; }

    [JsonPropertyName("dark")]
    public bool Dark { get; set; }

    [JsonPropertyName("subscription_preview")]
    public bool SubscriptionPreview { get; set; }

    [JsonPropertyName("button_color")]
    public string ButtonColor { get; set; } = string.Empty;
}