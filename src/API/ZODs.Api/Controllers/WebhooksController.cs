using ZODs.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ZODs.Payment.Configuration;
using ZODs.Payment.Models.Webhook;
using System.Text.Json;
using ZODs.Api.Service;
using ZODs.Api.Service.Extensions.InputMapping;
using ZODs.Api.Service.InputDtos.User;
using ZODs.Api.Common.Attributes;

namespace ZODs.Api.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
[AllowNoSubscription]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly ILogger<WebhooksController> logger;
    private readonly PaymentConfiguration paymentConfiguration;
    private readonly IPricingPlanService pricingPlanService;
    private readonly IUserAICreditBalanceService userAICreditBalanceService;
    private readonly IUsersService usersService;

    public WebhooksController(
        ILogger<WebhooksController> logger,
        IOptions<PaymentConfiguration> options,
        IPricingPlanService pricingPlanService,
        IUsersService usersService,
        IUserAICreditBalanceService userAICreditBalanceService)
    {
        this.logger = logger;
        paymentConfiguration = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(paymentConfiguration.SignatureSecret))
        {
            throw new ArgumentNullException(nameof(options), "Payment processor signature secret is empty.");
        }

        this.pricingPlanService = pricingPlanService;
        this.usersService = usersService;
        this.userAICreditBalanceService = userAICreditBalanceService;
    }

    [HttpPost("subscriptions/created")]
    public async Task<IActionResult> HandleSubscriptionCreated(CancellationToken cancellationToken)
    {
        var body = await Request.GetRawBodyStringAsync();
        Request.EnsureValidWebhookSignature(body, paymentConfiguration.SignatureSecret);

        var webhookPayload = JsonSerializer.Deserialize<PaymentSubscriptionWebhookPayload>(body);
        var subscriptionAttributes = webhookPayload.Data.Attributes;

        var subscriptionId = webhookPayload.Data.Id;
        logger.LogInformation(
            "Received subscription created webhook for subscription with id {id}.",
            subscriptionId);

        var userIdString = webhookPayload.Meta.CustomData.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userIdString))
        {
            logger.LogInformation(
                       "Received subscription created webhook for subscription with id {id} but no user id was provided.",
                       subscriptionId);

            return BadRequest();
        }

        var userId = Guid.Parse(userIdString);
        var upserUserPricingPlanDto = webhookPayload.ToUpsertUserPricingPlanInputDto();
        var hasUserPricingPlan = await pricingPlanService.HasUserPricingPlanAssignedAsync(userId, cancellationToken);

        if (hasUserPricingPlan)
        {
            // If the user already has a pricing plan assigned, we update it.
            // This covers the case when the user has a free trial and then subscribes to a paid plan.
            await pricingPlanService.UpdateUserPricingPlanAsync(upserUserPricingPlanDto, cancellationToken);
        }
        else
        {
            // If the user doesn't have a pricing plan assigned, we create a new one.
            await pricingPlanService.AssignPricingPlanToUserAsync(upserUserPricingPlanDto, cancellationToken);
        }

        var pricingPlanType = await pricingPlanService.GetPricingPlanTypeByVariantIdAsync(upserUserPricingPlanDto.VariantId, cancellationToken);

        // Assign AI credits for the user, by the pricing plan type
        await userAICreditBalanceService.SetUserAICreditsBalanceByPricingPlanAsync(userId, pricingPlanType, cancellationToken);

        var userBillingData = new UpdateUserBillingDataInputDto(
              subscriptionAttributes.CardLastFour,
              subscriptionAttributes.CardBrand,
              webhookPayload.Data.Attributes.Urls.UpdatePaymentMethod);

        await usersService.UpdateUserBillingDataAsync(userId, userBillingData);

        return Ok();
    }

    [HttpPost("subscriptions/updated")]
    public async Task<IActionResult> HandleSubscriptionUpdated(CancellationToken cancellationToken)
    {
        var body = await Request.GetRawBodyStringAsync();
        Request.EnsureValidWebhookSignature(body, paymentConfiguration.SignatureSecret);

        var webhookPayload = JsonSerializer.Deserialize<PaymentSubscriptionWebhookPayload>(body);
        var subscriptionAttributes = webhookPayload.Data.Attributes;

        var subscriptionId = webhookPayload.Data.Id;
        logger.LogInformation(
                           "Received subscription updated webhook for subscription with id {id}.",
                           subscriptionId);

        var userIdString = webhookPayload.Meta.CustomData.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userIdString))
        {
            logger.LogInformation(
                       "Received subscription created webhook for subscription with id {id} but no user id was provided.",
                       subscriptionId);

            return BadRequest();
        }

        var upserUserPricingPlanDto = webhookPayload.ToUpsertUserPricingPlanInputDto();

        await pricingPlanService.UpdateUserPricingPlanAsync(upserUserPricingPlanDto, cancellationToken);

        var userId = Guid.Parse(userIdString);
        var userBillingData = new UpdateUserBillingDataInputDto(
        subscriptionAttributes.CardLastFour,
              subscriptionAttributes.CardBrand,
              webhookPayload.Data.Attributes.Urls.UpdatePaymentMethod);

        await usersService.UpdateUserBillingDataAsync(userId, userBillingData);

        return Ok();
    }

    [HttpPost("orders/created")]
    public async Task<IActionResult> HandleOrderCreated(CancellationToken cancellationToken)
    {
        var body = await Request.GetRawBodyStringAsync();
        Request.EnsureValidWebhookSignature(body, paymentConfiguration.SignatureSecret);

        var webhookPayload = JsonSerializer.Deserialize<PaymentOrderWebhookPayload>(body);
        var orderAttributes = webhookPayload.Data.Attributes;
        var variantId = orderAttributes.FirstOrderItem.VariantId;

        var gpt3AICreditsVariantId = int.Parse(paymentConfiguration.Gpt3AICreditsVariantId);
        var gpt4AICreditsVariantId = int.Parse(paymentConfiguration.Gpt4AICreditsVariantId);

        if (variantId == gpt3AICreditsVariantId ||
            variantId == gpt4AICreditsVariantId)
        {
            return await HandleAICreditsOrderCreatedAsync(webhookPayload, cancellationToken);
        }

        return Ok();
    }

    private async Task<IActionResult> HandleAICreditsOrderCreatedAsync(
        PaymentOrderWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        var orderAttributes = payload.Data.Attributes;
        var creditsVariantId = orderAttributes.FirstOrderItem.VariantId;

        if (orderAttributes.Status != "paid")
        {
            // We only care about paid orders
            return Ok();
        }

        var orderId = payload.Data.Id;
        logger.LogInformation(
                      "Received order created webhook for order with id {id}.",
                      orderId);

        var userIdString = payload.Meta.CustomData.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userIdString))
        {
            logger.LogInformation(
                        "Received order created webhook for order with id {id} but no user id was provided.",
                        orderId);

            return BadRequest();
        }

        var userId = Guid.Parse(userIdString);
        var totalCreditsStringValue = payload.Meta.CustomData.ElementAt(1);

        if (!int.TryParse(totalCreditsStringValue, out var totalCredits))
        {
            logger.LogInformation(
                       "Received order created webhook for order with id {id} but no total credits was provided.",
                       orderId);

            return BadRequest();
        }

        var gpt3AICreditsVariantId = int.Parse(paymentConfiguration.Gpt3AICreditsVariantId);
        var gpt4AICreditsVariantId = int.Parse(paymentConfiguration.Gpt4AICreditsVariantId);

        if (creditsVariantId == gpt3AICreditsVariantId)
        {
            await userAICreditBalanceService.AddUserGpt3CreditsAsync(userId, totalCredits, cancellationToken);
        }
        else if (creditsVariantId == gpt4AICreditsVariantId)
        {
            await userAICreditBalanceService.AddUserGpt4CreditsAsync(userId, totalCredits, cancellationToken);
        }

        return Ok();
    }
}
