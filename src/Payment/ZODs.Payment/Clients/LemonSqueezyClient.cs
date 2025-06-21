using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZODs.Common.Exceptions;
using ZODs.Payment.Configuration;
using ZODs.Payment.Constants;
using ZODs.Payment.InputModels;
using ZODs.Payment.InputModels.Checkout;
using ZODs.Payment.Models.Checkout;
using ZODs.Payment.Models.Product;
using ZODs.Payment.Models.Subscription;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZODs.Payment.Clients;

public class LemonSqueezyClient(
        ILogger<LemonSqueezyClient> logger,
        IOptions<PaymentConfiguration> options,
        IHttpClientFactory httpClientFactory) : IPaymentProcessorClient
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly ILogger<LemonSqueezyClient> logger = logger;
    private readonly PaymentConfiguration paymentConfiguration = options.Value;

    private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
    {
        // IMPORTANT! This is required for the json to be serialized correctly.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<PaymentCheckoutPayload?> CreatePricingPlanPaymentCheckoutAsync(
        int paymentVariantId,
        string userId,
        string email,
        string? discountCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inputModel = CheckoutCreateInputModel.Create(
               storeId: paymentConfiguration.StoreId,
               paymentVariantId,
               email,
               new[] { userId },
               customPrice: null,
               discountCode,
               new PaymentProductOptions
               {
                   RedirectUrl = paymentConfiguration.PricingPlanCheckoutRedirectUrl,
               },
               testMode: paymentConfiguration.TestMode);

            var jsonInput = JsonSerializer.Serialize(inputModel, JSON_SERIALIZER_OPTIONS);

            var content = new StringContent(jsonInput, Encoding.UTF8, LemonSqueezyConstants.ContentType);

            var client = GetClient();

            var response = await client.PostAsync("/v1/checkouts", content, cancellationToken);

            await EnsureSuccessStatusCodeOnCheckoutResponse(response, userId, paymentVariantId, "create");

            var payload = await response.Content.ReadFromJsonAsync<PaymentCheckoutPayload>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                await LogDeserializationError(response, userId, "create");
                throw new PaymentException($"Failed to create checkout for user with id {userId} and variant {paymentVariantId}.");
            }

            return payload;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception encountered while trying to create pricing plan checkout for user with id {UserId} and variant {VariantId}.", userId, paymentVariantId);
            return null;
        }
    }

    public async Task<PaymentCheckoutPayload?> CreateAICreditsPaymentCheckoutAsync(
        int paymentVariantId,
        decimal valueToPay,
        int totalCredits,
        string userId,
        string email,
        string description,
        string? discountCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inputModel = CheckoutCreateInputModel.Create(
               storeId: paymentConfiguration.StoreId,
               paymentVariantId,
               email,
               new[] { userId, totalCredits.ToString() },
               valueToPay,
               discountCode,
               new PaymentProductOptions
               {
                   Description = description,
                   RedirectUrl = paymentConfiguration.CreditsCheckoutRedirectUrl,
               },
               testMode: paymentConfiguration.TestMode);

            var jsonInput = JsonSerializer.Serialize(inputModel, JSON_SERIALIZER_OPTIONS);
            var content = new StringContent(jsonInput, Encoding.UTF8, LemonSqueezyConstants.ContentType);

            var client = GetClient();
            var response = await client.PostAsync("/v1/checkouts", content, cancellationToken);

            await EnsureSuccessStatusCodeOnCheckoutResponse(response, userId, paymentVariantId, "create");

            var payload = await response.Content.ReadFromJsonAsync<PaymentCheckoutPayload>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                await LogDeserializationError(response, userId, "create");
                throw new PaymentException($"Failed to create checkout for user with id {userId} and variant {paymentVariantId}.");
            }

            return payload;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Exception encountered while trying to create AI credits checkout for user with id {UserId} and variant {VariantId}.", userId, paymentVariantId);
            return null;
        }
    }

    public async Task<PaymentSubscriptionPayload?> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/v1/subscriptions/{subscriptionId}", cancellationToken);

            await EnsureSuccessStatusCodeOnSubscriptionResponse(
                    response,
                    subscriptionId,
                    "cancel subscription");

            var payload = await response.Content.ReadFromJsonAsync<PaymentSubscriptionPayload>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                await LogDeserializationError(response, subscriptionId, "cancel subscription");
                return null;
            }

            return payload;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Exception encountered while trying to cancel subscription {SubscriptionId}.", subscriptionId);
            return null;
        }
    }

    public async Task<PaymentSubscriptionPayload?> ResumeSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionPatchModel = SubscriptionPatchModel.CreateCancelModel(subscriptionId, false);
            var jsonInput = JsonSerializer.Serialize(subscriptionPatchModel);
            var content = new StringContent(jsonInput, Encoding.UTF8, LemonSqueezyConstants.ContentType);

            var client = GetClient();
            var response = await client.PatchAsync($"/v1/subscriptions/{subscriptionId}", content, cancellationToken);

            await EnsureSuccessStatusCodeOnSubscriptionResponse(
                    response,
                    subscriptionId,
                    "cancel subscription");

            var payload = await response.Content.ReadFromJsonAsync<PaymentSubscriptionPayload>();
            if (payload is null)
            {
                await LogDeserializationError(response, subscriptionId, "resume subscription");
                return null;
            }

            return payload;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Exception encountered while trying to resume subscription {SubscriptionId}.", subscriptionId);
            return null;
        }
    }

    private HttpClient GetClient()
    {
        return httpClientFactory.CreateClient(HttpClients.PaymentProcessorClient) ?? throw new InvalidOperationException("Failed to instantiate http client.");
    }

    private async Task EnsureSuccessStatusCodeOnSubscriptionResponse(
        HttpResponseMessage response,
        string subscriptionId,
        string actionMessage)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            logger.LogError(
                    "Failed to {actionMessage} subscription {SubscriptionId}. Status Code: {StatusCode}. Content: {Content}",
                    actionMessage,
                    subscriptionId,
                    response.StatusCode,
                    content);

            throw new PaymentException($"Failed to {actionMessage} subscription {subscriptionId}. Status Code: {response.StatusCode}");
        }
    }

    private async Task EnsureSuccessStatusCodeOnCheckoutResponse(
         HttpResponseMessage response,
         string userId,
         int variantId,
         string actionMessage)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            logger.LogError(
                    "Failed to {actionMessage} checkout for user with id {userId} and variant {variantId}. Status Code: {StatusCode}. Content: {Content}",
                    actionMessage,
                    userId,
                    variantId,
                    response.StatusCode,
                    content);

            throw new PaymentException($"Failed to {actionMessage} checkout.");
        }
    }

    private async Task LogDeserializationError(
        HttpResponseMessage response,
        string subscriptionId,
        string actionMessage)
    {
        var content = await response.Content.ReadAsStringAsync();
        logger.LogError(
                 "Failed to deserialize response from {actionMessage} {SubscriptionId}. Content: {content}",
                 actionMessage,
                 subscriptionId,
                 content);
    }
}