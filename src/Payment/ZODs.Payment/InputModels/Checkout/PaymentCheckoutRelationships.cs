using System.Text.Json.Serialization;

namespace ZODs.Payment.InputModels.Checkout;

public sealed class PaymentCheckoutRelationships
{
    [JsonPropertyName("store")]
    public Relationship Store { get; set; }

    [JsonPropertyName("variant")]
    public Relationship Variant { get; set; }

    public PaymentCheckoutRelationships(Relationship store, Relationship variant)
    {
        Store = store;
        Variant = variant;
    }
}