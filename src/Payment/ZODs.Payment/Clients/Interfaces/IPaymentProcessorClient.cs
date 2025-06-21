using ZODs.Payment.Models.Checkout;
using ZODs.Payment.Models.Subscription;

namespace ZODs.Payment.Clients;

public interface IPaymentProcessorClient
{
    Task<PaymentSubscriptionPayload?> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

    Task<PaymentCheckoutPayload?> CreateAICreditsPaymentCheckoutAsync(
        int paymentVariantId, 
        decimal valueToPay, 
        int totalCredits, 
        string userId, 
        string email, 
        string description,
        string? discountCode = null,
        CancellationToken cancellationToken = default);

    Task<PaymentCheckoutPayload?> CreatePricingPlanPaymentCheckoutAsync(
        int paymentVariantId, 
        string userId,
        string email, 
        string? discountCode = null, 
        CancellationToken cancellationToken = default);

    Task<PaymentSubscriptionPayload?> ResumeSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
}