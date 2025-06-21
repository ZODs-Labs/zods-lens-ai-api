using Microsoft.AspNetCore.Mvc;
using ZODs.AI.Common;
using ZODs.AI.Common.Enums;
using ZODs.AI.Common.Interfaces;
using ZODs.AI.Common.Models;
using ZODs.AI.Google.InputDtos;
using ZODs.AI.Together;
using ZODs.AI.Together.InputDtos;
using ZODs.AI.Together.Models;
using ZODs.Api.Authorization;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Common.Enums;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Filters;
using ZODs.Api.Service;
using ZODs.Common.Extensions;
using System.Text;
using System.Text.Json;

namespace ZODs.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AIController
    (ITogetherAIClient togetherAIClient,
    IAILensService aiLensService,
    IAIContextService aiContextService) : BaseController
{
    private readonly ITogetherAIClient togetherAIClient = togetherAIClient;
    private readonly IAILensService aiLensService = aiLensService;
    private readonly IAIContextService aiContextService = aiContextService;

    [NonAction]
    [PaidPlanOnly]
    [ValidSubscription]
    [LogRequest]
    [RateLimitByPlan(RateLimitationType.Mixtral8x7b)]
    [HttpPost("mixtral/code/completion")]
    public async Task GetChatCompletionWithMixtral(
            [FromBody] Mixtral8x7BChatCompletionInputDto inputDto,
            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        var chatId = inputDto.ChatId;
        var isChatIdProvided = chatId.HasValue;

        var mainInstructions = GetMainInstructions(inputDto.ContextCode, inputDto.FileExtension);
        var messages = new List<IAIChatMessage>
        {
            new TogetherMessage(mainInstructions, ContextChatRole.System),
        };

        if (isChatIdProvided)
        {
            var contextMessages = await GetContextMessagesAsync(chatId.Value);
            messages.AddRange(contextMessages);
        }

        var prompt = inputDto.Prompt;
        if (!string.IsNullOrWhiteSpace(inputDto.ContextCode))
        {
            prompt += $"Consider this code as a context: {inputDto.ContextCode}";
        }

        messages.Add(new TogetherMessage(prompt, ContextChatRole.User));

        var options = new TogetherAIRequestOptions
        {
            AiModel = inputDto.AiModel,
            MaxTokens = inputDto.MaxTokens,
            StreamTokens = true,
            ChatMessages = messages,
        };

        var outputContent = await PromptAndStreamChatCompletion(options, cancellationToken: cancellationToken);

        if (isChatIdProvided)
        {
            var userMessage = new AIContextMessage(inputDto.Prompt, ContextChatRole.User);
            var assistantMessage = new AIContextMessage(outputContent, ContextChatRole.Assistant);
            var ctxMessages = new[] { userMessage, assistantMessage };
            aiContextService.AddMessagesToContextAsBackgroundTaskAsync(chatId.Value, ctxMessages);
        }

        HttpContext.Items.Add("RequestLogMetadata", JsonSerializer.Serialize(new
        {
            PromptLength = inputDto.Prompt.Length,
            CodeContextLength = inputDto.ContextCode?.Length ?? 0,
        }));
    }

    [NonAction]
    public async Task GetGeminiChatCompletion(
        [FromBody] GeminiProChatCompletionInputDto inputDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        var chatId = inputDto.ChatId;
        var isChatIdProvided = chatId.HasValue;

        var mainInstructions = GetMainInstructions(inputDto.ContextCode, inputDto.FileExtension);
        var messages = new List<IAIChatMessage>
        {
            new TogetherMessage(mainInstructions, ContextChatRole.System),
        };

        if (isChatIdProvided)
        {
            var contextMessages = await GetContextMessagesAsync(chatId.Value);
            messages.AddRange(contextMessages);
        }

        var prompt = inputDto.Prompt;
        if (!string.IsNullOrWhiteSpace(inputDto.ContextCode))
        {
            prompt += $"Consider this code as a context: {inputDto.ContextCode}";
        }

        messages.Add(new TogetherMessage(prompt, ContextChatRole.User));

        var options = new TogetherAIRequestOptions
        {
            AiModel = inputDto.AiModel,
            MaxTokens = inputDto.MaxTokens,
            StreamTokens = true,
            ChatMessages = messages,
        };

        var outputContent = await PromptAndStreamChatCompletion(options, cancellationToken: cancellationToken);

        if (isChatIdProvided)
        {
            var userMessage = new AIContextMessage(inputDto.Prompt, ContextChatRole.User);
            var assistantMessage = new AIContextMessage(outputContent, ContextChatRole.Assistant);
            var ctxMessages = new[] { userMessage, assistantMessage };
            aiContextService.AddMessagesToContextAsBackgroundTaskAsync(inputDto.ChatId.Value, ctxMessages);
        }

        HttpContext.Items.Add("RequestLogMetadata", JsonSerializer.Serialize(new
        {
            PromptLength = inputDto.Prompt.Length,
            CodeContextLength = inputDto.ContextCode?.Length ?? 0,
        }));
    }

    [NonAction]
    [PaidPlanOnly]
    [ValidSubscription]
    [LogRequest]
    [RateLimitByPlan(RateLimitationType.Mixtral8x7b)]
    [HttpPost("mixtral/code/ai-lens")]
    public async Task GetAILensCompletionWithMixtral(
        [FromBody] Mixtral8x7BAILensCompletionInputDto inputDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        var chatId = inputDto.ChatId;
        var isChatIdProvided = chatId.HasValue;

        var mainInstructions = GetMainInstructions(inputDto.ContextCode, inputDto.FileExtension);
        var lensInstructions = await aiLensService.GetAILensInstructionsAsync(inputDto.AILensId, cancellationToken: cancellationToken);


        var lensInstructionsString = @$"
Behavior instruction: {lensInstructions.BehaviorInstruction}.
";

        var allSystemInstructions = new StringBuilder();
        allSystemInstructions.Append(mainInstructions);
        allSystemInstructions.Append(lensInstructionsString);

        var messages = new List<IAIChatMessage>
        {
            new TogetherMessage(allSystemInstructions.ToString(), ContextChatRole.System),
        };

        if (isChatIdProvided)
        {
            var contextMessages = await GetContextMessagesAsync(chatId.Value);
            messages.AddRange(contextMessages);
        }

        messages.Add(new TogetherMessage(lensInstructions.ResponseInstruction, ContextChatRole.User));

        var options = new TogetherAIRequestOptions
        {
            AiModel = inputDto.AiModel,
            MaxTokens = inputDto.MaxTokens,
            StreamTokens = true,
            ChatMessages = messages,
        };

        var outputContent = await PromptAndStreamChatCompletion(options, cancellationToken: cancellationToken);

        if (isChatIdProvided)
        {
            var userMessage = new AIContextMessage(inputDto.ContextCode, ContextChatRole.User);
            var assistantMessage = new AIContextMessage(outputContent, ContextChatRole.Assistant);
            var ctxMessages = new[] { userMessage, assistantMessage };
            aiContextService.AddMessagesToContextAsBackgroundTaskAsync(inputDto.ChatId.Value, ctxMessages);
        }

        HttpContext.Items.Add("RequestLogMetadata", JsonSerializer.Serialize(new
        {
            CodeContextLength = inputDto.ContextCode?.Length ?? 0,
        }));
    }

    private async Task<string> PromptAndStreamChatCompletion(
        TogetherAIRequestOptions aiOptions,
        bool bufferContent = true,
        CancellationToken cancellationToken = default)
    {
        StreamingResponse<TogetherAICompletion> completionResponse;
        completionResponse = await togetherAIClient.GetCompletionsStreamingAsync(
                aiOptions,
                cancellationToken);

        return await Response.StreamChatCompletionsAsync(
            completionResponse,
            bufferContent: bufferContent,
            cancellationToken: cancellationToken);
    }

    private static string GetMainInstructions(string codeContext, string fileExtension)
    {
        var builder = new StringBuilder();
        builder.Append(AIInstructions.GlobalCodingInstruction);

        if (!string.IsNullOrWhiteSpace(codeContext))
        {
            builder.Append($" Consider this code as a context: {codeContext}.");
        }

        if (!string.IsNullOrWhiteSpace(fileExtension))
        {
            builder.Append($"The following is a programming language extension of provided code snippet: {fileExtension}");
        }

        return builder.ToString();
    }

    private async Task<ICollection<AIContextMessage>> GetContextMessagesAsync(Guid chatId)
    {
        var messages = await aiContextService.GetContextMessages(chatId);
        return messages;
    }
}
