using ZODs.Payment.Models.Product;
using System.Text.Json.Serialization;

namespace ZODs.Payment.InputModels.Checkout;

public sealed class PaymentCheckoutData(
    string email,
    ICollection<string> custom,
    string? discountCode = null)
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = email;

    [JsonPropertyName("discount_code")]
    public string? DiscountCode { get; set; } = string.IsNullOrWhiteSpace(discountCode) ? null : discountCode;

    [JsonPropertyName("custom")]
    public ICollection<string> Custom { get; set; } = custom;
}

public sealed class PaymentCheckoutAttributesInputModel(
    PaymentCheckoutData data,
    PaymentProductOptions? productOptions,
    PaymentCheckoutOptions? checkoutOptions,
    decimal? customPrice,
    bool testMode)
{
    [JsonPropertyName("custom_price")]
    public decimal? CustomPrice { get; set; } = customPrice;

    [JsonPropertyName("checkout_data")]
    public PaymentCheckoutData CheckoutData { get; set; } = data;

    [JsonPropertyName("product_options")]
    public PaymentProductOptions? ProductOptions { get; set; } = productOptions;

    [JsonPropertyName("checkout_options")]
    public PaymentCheckoutOptions? CheckoutOptions { get; set; } = checkoutOptions;

    [JsonPropertyName("test_mode")]
    public bool TestMode { get; set; } = testMode;
}