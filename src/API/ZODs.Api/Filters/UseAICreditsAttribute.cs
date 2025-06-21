using Microsoft.AspNetCore.Mvc.Filters;
using ZODs.AI.OpenAI.Utils;
using ZODs.Api.Extensions;
using ZODs.Api.Service;

namespace ZODs.Api.Filters;

public sealed class UseAICreditsAttribute : ActionFilterAttribute
{
    public UseAICreditsAttribute()
    {
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        // Resolve the service from the scope
        var creditService = context.HttpContext.RequestServices.GetRequiredService<IUserAICreditBalanceService>();

        // Extract the userId from the request context
        var userId = httpContext.User.GetUserId();
        if (userId == default)
        {
            LogCritical("Failed to extract userId from the request context.", userId, context.HttpContext.RequestServices);
            return;
        }

        // Extract the used tokens from the response header
        var usedTokens = httpContext.GetUsedTokensHeader();
        if (usedTokens < 0)
        {
            LogCritical("Detected negative used tokens.", userId, context.HttpContext.RequestServices);
        }

        var usedCredits = OpenAICreditsCalculator.ConvertOpenAITokensToCredits(usedTokens);

        var aiModel = httpContext.GetAIModelHeader();
        if (string.IsNullOrWhiteSpace(aiModel))
        {
            LogCritical("Failed to extract AI model from the response header.", userId, context.HttpContext.RequestServices);
            return;
        }

        // Check the balance based on the AI model specified
        if (aiModel.StartsWith("gpt-4o-mini"))
        {
            await creditService.UseUserGpt3CreditsAsync(userId, usedCredits, httpContext.RequestAborted);
        }
        else if (aiModel.StartsWith("gpt-4"))
        {
            await creditService.UseUserGpt4CreditsAsync(userId, usedCredits, httpContext.RequestAborted);
        }
        else
        {
            LogCritical("Invalid AI model header.", userId, context.HttpContext.RequestServices);
        }

        // Continue with the execution of the filter pipeline
        await next();
    }

    private static void LogCritical(string message, Guid userId, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<UseAICreditsAttribute>>();
        logger.LogCritical("Critical: {message}. UserId: {userId}", message, userId);
    }
}
