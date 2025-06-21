using ZODs.Payment.InputModels.Checkout;
using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Checkout;

public sealed class PaymentCheckoutData : PaymentData<PaymentCheckoutAttributes>
{
    [JsonPropertyName("checkout_options")]
    public PaymentCheckoutOptions? CheckoutOptions { get; set; }

    [JsonPropertyName("checkout_data")]
    public PaymentCheckoutData? CheckoutData { get; set; }
}