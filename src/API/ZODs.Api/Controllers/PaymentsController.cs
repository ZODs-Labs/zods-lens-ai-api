using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ZODs.AI.OpenAI.Utils;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Common.Constants;
using ZODs.Api.Extensions;
using ZODs.Api.Models.Input.Payment;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Validation;
using ZODs.Payment.Clients;
using ZODs.Payment.Configuration;

namespace ZODs.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(
    IPaymentProcessorClient paymentProcessorClient,
    IPricingPlanService pricingPlanService,
    IOptions<PaymentConfiguration> options) : BaseController
{
    private readonly IPaymentProcessorClient paymentProcessorClient = paymentProcessorClient;
    private readonly IPricingPlanService pricingPlanService = pricingPlanService;
    private readonly PaymentConfiguration paymentConfiguration = options.Value;

    [AllowNoSubscription]
    [HttpPost("checkouts/plan")]
    public async Task<IActionResult> CreatePaymentCheckout(
        [FromBody] CreateCheckoutInputDto inputDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var email = User.GetUserEmail();

        var pricingPlanVariantId = await pricingPlanService.GetPricingPlanPaymentVariantIdAsync(
                inputDto.PricingPlanType,
                inputDto.PricingPlanVariantType,
                cancellationToken);

        var discountCode = inputDto.PromoCode;
        //if (inputDto.PricingPlanType == PricingPlanType.Basic &&
        //    inputDto.PricingPlanVariantType == PricingPlanVariantType.Monthly)
        //{
        //    // Temporary discount code for pre-launch
        //    discountCode = "ZODSPRELAUNCH";
        //}

        var paymentCheckoutPayload = await paymentProcessorClient.CreatePricingPlanPaymentCheckoutAsync(
            pricingPlanVariantId,
            userId.ToString(),
            email,
            discountCode,
            cancellationToken);

        var checkoutUrl = paymentCheckoutPayload?.Data?.Attributes?.Url;
        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return BadRequest("Failed to create payment checkout");
        }

        return OkApiResponse(checkoutUrl);
    }

    [ValidateFeature(FeatureKeys.AIGpt3, requiredPaidPlan: true)]
    [HttpPost("checkouts/credits/gpt3")]
    public async Task<IActionResult> CreateGpt3AICreditsCheckout(
         [FromBody] CreateGpt3AICreditsPaymentCheckoutInputDto inputDto,
         CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var email = User.GetUserEmail();
        var variantId = int.Parse(paymentConfiguration.Gpt3AICreditsVariantId);

        var credits = inputDto.CreditsAmount;

        var creditsPrice = OpenAICreditsCalculator.CalculateGpt3CreditsPrice(credits);
        var creditsPriceCents = creditsPrice * 100;

        var paymentCheckoutPayload = await paymentProcessorClient
             .CreateAICreditsPaymentCheckoutAsync(
                 paymentVariantId: variantId,
                 valueToPay: creditsPriceCents,
                 totalCredits: credits,
                 userId.ToString(),
                 email,
                 description: $"This is one-time payment for {credits:N0} of GPT-4o Mini AI credits.",
                 cancellationToken: cancellationToken);

        var checkoutUrl = paymentCheckoutPayload?.Data?.Attributes?.Url;
        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return BadRequest("Failed to create payment checkout");
        }

        return OkApiResponse(checkoutUrl);
    }

    [ValidateFeature(FeatureKeys.AIGpt4, requiredPaidPlan: true)]
    [HttpPost("checkouts/credits/gpt4")]
    public async Task<IActionResult> CreateGpt4AICreditsCheckout(
         [FromBody] CreateGpt4AICreditsPaymentCheckoutInputDto inputDto,
         CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var email = User.GetUserEmail();
        var variantId = int.Parse(paymentConfiguration.Gpt4AICreditsVariantId);

        var credits = inputDto.CreditsAmount;

        var creditsPrice = OpenAICreditsCalculator.CalculateGpt4CreditsPrice(credits);
        var creditsPriceCents = creditsPrice * 100;

        var paymentCheckoutPayload = await paymentProcessorClient
             .CreateAICreditsPaymentCheckoutAsync(
                 paymentVariantId: variantId,
                 valueToPay: creditsPriceCents,
                 totalCredits: credits,
                 userId.ToString(),
                 email,
                 description: $"Thie is one-time payment for {credits:N0} of GPT-4 AI credits.",
                 cancellationToken: cancellationToken);

        var checkoutUrl = paymentCheckoutPayload?.Data?.Attributes?.Url;
        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return BadRequest("Failed to create payment checkout");
        }

        return OkApiResponse(checkoutUrl);
    }
}
