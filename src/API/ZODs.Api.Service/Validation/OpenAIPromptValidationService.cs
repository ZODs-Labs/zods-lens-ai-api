using Azure.AI.OpenAI;
using ZODs.AI.OpenAI.Constants;
using ZODs.AI.OpenAI.Interfaces;
using ZODs.AI.OpenAI.Utils;
using ZODs.Api.Service.Validation.Interfaces;
using ZODs.Common.Exceptions;
using ZODs.Common.Extensions;

namespace ZODs.Api.Service.Validation;

public sealed class OpenAIPromptValidationService : IOpenAIPromptValidationService
{
    private readonly ITokenizerService tokenizerService;
    private readonly IUserAICreditBalanceService userAICreditBalanceService;

    public OpenAIPromptValidationService(
        ITokenizerService tokenizerService,
        IUserAICreditBalanceService userAICreditBalanceService)
    {
        this.tokenizerService = tokenizerService;
        this.userAICreditBalanceService = userAICreditBalanceService;
    }

    public async Task<int> ValidateUserOpenAIPromptAsync(
        ICollection<ChatMessage> messages,
        string model,
        int maxOutputTokens,
        Guid userId,
        bool isPrompt = true,
        CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
        {
            // No messages - return 0 used tokens
            return 0;
        }

        var modelCreditsBalance = await GetUserAIModelCreditsBalanceAsync(model, userId, cancellationToken).NoSync();
        if (modelCreditsBalance == 0)
        {
            throw new InsufficientAICreditsException("You have no credits left. Don’t let progress pause — [add credits](https://app.zods.pro/credits) now to sustain your smooth coding journey with AI.");
        }

        var modelTokensBalance = OpenAICreditsCalculator.ConvertOpenAICreditsToTokens(modelCreditsBalance);

        var inputContent = string.Join("\n", messages.Select(x => x.Content));

        var inputTokens = tokenizerService.CountTokensFromString(model, inputContent);
        ValidateModelMaxTokens(model, inputTokens, maxOutputTokens);

        var totalExpectedTokens = inputTokens + maxOutputTokens;
        var exceedingTokens = totalExpectedTokens - modelTokensBalance;

        if (exceedingTokens > 0)
        {
            var exceedingCredits = OpenAICreditsCalculator.ConvertOpenAITokensToCredits(exceedingTokens);
            var message = isPrompt ?
                $"Oops! Your prompt is too long for the remaining credits (you need {exceedingCredits} more). Your current balance is {modelCreditsBalance} credits. Please shorten your prompt, decrease max tokens (output) or [add more credits](https://app.zods.pro/credits)." :
                $"Oops! This operation is heavy for the remaining credits (you need {exceedingCredits} more).  \nYour current balance is {modelCreditsBalance} credits. Please {(modelTokensBalance > 0 ? "adjust inputs or " : " ")}[add more credits](https://app.zods.pro/credits).";

            throw new ClientSideException(message);
        }

        return inputTokens;
    }

    private static void ValidateModelMaxTokens(string model, int inputTokens, int maxOutputTokens)
    {
        var totalTokens = inputTokens + maxOutputTokens;

        if (OpenAIModelMaxTokens.ModelMaxTokens.TryGetValue(model, out var modelMaxTokens))
        {
            if (totalTokens > modelMaxTokens)
            {
                throw new ClientSideException($"Oops! Your prompt is too long for the selected model. The maximum number of tokens for this model is {modelMaxTokens} tokens. Please shorten your prompt, decrease max tokens or select another model.");
            }
        }
    }

    private async Task<int> GetUserAIModelCreditsBalanceAsync(string model, Guid userId, CancellationToken cancellationToken)
    {
        if (model.StartsWith("gpt-4o-mini"))
        {
            return await userAICreditBalanceService.GetUserGpt3CreditBalanceAsync(userId, cancellationToken);
        }
        else if (model.StartsWith("gpt-4"))
        {
            return await userAICreditBalanceService.GetUserGpt4CreditBalanceAsync(userId, cancellationToken);
        }
        else
        {
            throw new BusinessValidationException("Invalid AI model header.");
        }
    }
}