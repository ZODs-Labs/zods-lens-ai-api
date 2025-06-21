namespace ZODs.Payment.Configuration;

public sealed class PaymentConfiguration
{
    public string ApiKey { get; set; } = null!;

    public string ApiUrl { get; set; } = null!;

    public string SignatureSecret { get; set; } = null!;

    public string StoreId { get; set; } = null!;

    public string Gpt3AICreditsVariantId { get; set; } = default!;

    public string Gpt4AICreditsVariantId { get; set; } = default!;

    public string PricingPlanCheckoutRedirectUrl { get; set; } = null!;

    public string CreditsCheckoutRedirectUrl { get; set; } = null!;

    public bool TestMode { get; set; }

    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }

        if (string.IsNullOrWhiteSpace(ApiUrl))
        {
            throw new ArgumentNullException(nameof(ApiUrl));
        }

        if (string.IsNullOrWhiteSpace(SignatureSecret))
        {
            throw new ArgumentNullException(nameof(SignatureSecret));
        }

        if (string.IsNullOrWhiteSpace(StoreId))
        {
            throw new ArgumentNullException(nameof(StoreId));
        }

        if (string.IsNullOrWhiteSpace(PricingPlanCheckoutRedirectUrl))
        {
            throw new ArgumentNullException(nameof(PricingPlanCheckoutRedirectUrl));
        }

        if (string.IsNullOrWhiteSpace(CreditsCheckoutRedirectUrl))
        {
            throw new ArgumentNullException(nameof(CreditsCheckoutRedirectUrl));
        }

        if (string.IsNullOrWhiteSpace(Gpt3AICreditsVariantId) || !int.TryParse(Gpt3AICreditsVariantId, out var _))
        {
            throw new ArgumentNullException(nameof(Gpt3AICreditsVariantId));
        }

        if (string.IsNullOrWhiteSpace(Gpt4AICreditsVariantId) || !int.TryParse(Gpt4AICreditsVariantId, out var _))
        {
            throw new ArgumentNullException(nameof(Gpt4AICreditsVariantId));
        }
    }
}