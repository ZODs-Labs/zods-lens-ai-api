using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ZODs.AI.Common;
using ZODs.AI.Common.Enums;
using ZODs.AI.Common.InputDtos.Interfaces;
using ZODs.AI.Common.Models;
using ZODs.AI.OpenAI.Constants;
using ZODs.AI.OpenAI.Dtos;
using ZODs.AI.OpenAI.Extensions;
using ZODs.AI.OpenAI.Interfaces;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Common.Constants;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Extensions;
using ZODs.Api.Filters;
using ZODs.Api.Service;
using ZODs.Api.Service.Validation.Interfaces;
using ZODs.Api.Validation;
using ZODs.Common.Extensions;
using ZODs.Common.Models;
using System.Text;
using System.Text.Json;

namespace ZODs.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GptController(
    IOpenAIChatCompletionService openAIChatCompletionService,
    IAILensService aiLensService,
    ITokenizerService tokenizerService,
    IOpenAIPromptValidationService openAIPromptValidationService,
    IAIContextService aIContextService) : BaseController
{
    private readonly IOpenAIChatCompletionService openAIChatCompletionService = openAIChatCompletionService;
    private readonly IOpenAIPromptValidationService openAIPromptValidationService = openAIPromptValidationService;
    private readonly IAILensService aiLensService = aiLensService;
    private readonly ITokenizerService tokenizerService = tokenizerService;
    private readonly IAIContextService aIContextService = aIContextService;

    private static readonly JsonSerializerOptions SERIAZLIER_OPTIONS = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [SuppressModelStateInvalidFilter]
    [ValidateFeature(FeatureKeys.AIGpt3, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [Produces("text/plain")]
    [HttpPost("4-mini/code/completion")]
    [UseStreamedAICredits]
    public async Task GetGpt3CompletionAsync(
            [FromBody] Gpt3ChatCompletionPromptInputDto inputDto,
            CancellationToken cancellationToken)
    {
        inputDto.AiModel = OpenAIModels.Gpt4o_Mini;
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        await HandleCodeCompletionPromptAsync(inputDto, null, countTokens: true, cancellationToken).NoSync();
    }

    [SuppressModelStateInvalidFilter]
    [ValidateFeature(FeatureKeys.AIGpt4, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [Produces("text/plain")]
    [HttpPost("4/code/completion")]
    [UseStreamedAICredits]
    public async Task GetGpt4CompletionAsync(
        [FromBody] Gpt4ChatCompletionPromptInputDto inputDto,
        CancellationToken cancellationToken)
    {
        inputDto.AiModel = OpenAIModels.Gpt4o;
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        await HandleCodeCompletionPromptAsync(inputDto, null, countTokens: true, cancellationToken).NoSync();
    }

    [SuppressModelStateInvalidFilter]
    [ValidateFeature(FeatureKeys.AIGpt3, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [HttpPost("4-mini/code/ai-lens")]
    [UseStreamedAICredits]
    public async Task GetGpt3AILensCompletionAsync(
        [FromBody] Gpt3AILensCompletionInputDto inputDto,
        CancellationToken cancellationToken)
    {
        inputDto.AiModel = OpenAIModels.Gpt4o_Mini;
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        await HandleAILensCodeCompletionPromptAsync(inputDto, null, countTokens: true, cancellationToken).NoSync();
    }

    [SuppressModelStateInvalidFilter]
    [ValidateFeature(FeatureKeys.AIGpt4, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [HttpPost("4/code/ai-lens")]
    [UseStreamedAICredits]
    public async Task GetGpt4AILensCompletionAsync(
        [FromBody] Gpt4AILensCompletionInputDto inputDto,
        CancellationToken cancellationToken)
    {
        inputDto.AiModel = OpenAIModels.Gpt4o;
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        await HandleAILensCodeCompletionPromptAsync(inputDto, null, countTokens: true, cancellationToken).NoSync();
    }

    [ValidateFeature(FeatureKeys.AIGpt3, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [HttpPost("4-mini/code/test/cases")]
    [UseAICredits]
    public async Task<IActionResult> GetGpt3UnitTestCasesAsync(
        [FromBody] Gpt3GenerateUnitTestCasesInputDto inputDto,
        CancellationToken cancellationToken)
    {
        var testCases = await GenerateUnitTestCasesAsync(
            inputDto,
            OpenAIModels.Gpt4o_Mini,
            4096,
            cancellationToken);

        return OkApiResponse(testCases);
    }

    [ValidateFeature(FeatureKeys.AIGpt4, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [HttpPost("4/code/test/cases")]
    [UseAICredits]
    public async Task<IActionResult> GetGpt4UnitTestCasesAsync(
        [FromBody] Gpt4GenerateUnitTestCasesInputDto inputDto,
        CancellationToken cancellationToken)
    {
        var testCases = await GenerateUnitTestCasesAsync(
            inputDto,
            OpenAIModels.Gpt4o,
            4000,
            cancellationToken);

        return OkApiResponse(testCases);
    }

    [ValidateFeature(FeatureKeys.AIGpt3, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [HttpPost("4-mini/code/test")]
    [UseAICredits]
    public async Task<IActionResult> Gpt3GenerateUnitTest(
        [FromBody] Gpt3GenerateUnitTestInputDto inputDto,
        CancellationToken cancellationToken)
    {
        var unitTest = await GenerateUnitTestAsync(
            inputDto,
            OpenAIModels.Gpt4o_Mini,
            4096,
            cancellationToken);

        return OkApiResponse(unitTest);
    }

    [ValidateFeature(FeatureKeys.AIGpt4, errorMessage: OpenAIValidationMessages.UnauthorizedGPT4Access)]
    [HttpPost("4/code/test")]
    [UseAICredits]
    public async Task<IActionResult> Gpt4GenerateUnitTest(
        [FromBody] Gpt4GenerateUnitTestInputDto inputDto,
        CancellationToken cancellationToken)
    {
        var unitTest = await GenerateUnitTestAsync(
            inputDto,
            OpenAIModels.Gpt4o,
            4096,
            cancellationToken);

        return OkApiResponse(unitTest);
    }

    [AllowNoSubscription]
    [EnableRateLimiting(RateLimiters.OpenAICustomApiKeyChatRateLimiter)]
    [SuppressModelStateInvalidFilter]
    //[ValidateFeature(FeatureKeys.CustomAiApiKeys, errorMessage: OpenAIValidationMessages.UnauthorizedCustomAIApiKeyFeatureAccess)]
    [HttpPost("own/code/completion")]
    public async Task GetCodeCompletionWithOwnApiKeyAsync(
        [FromBody] ChatCompletionPromptWithApiKeyInputDto inputDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        await HandleCodeCompletionPromptAsync(inputDto, apiKey: inputDto.ApiKey, countTokens: false, cancellationToken).NoSync();
    }

    [AllowNoSubscription]
    [EnableRateLimiting(RateLimiters.OpenAICustomApiKeyAILensRateLimiter)]
    [SuppressModelStateInvalidFilter]
    //[ValidateFeature(FeatureKeys.CustomAiApiKeys, errorMessage: OpenAIValidationMessages.UnauthorizedCustomAIApiKeyFeatureAccess)]
    [HttpPost("own/code/ai-lens")]
    public async Task GeAILensCompletionWithOwnApiKeyAsync(
        [FromBody] AILensCompletionPromptWithApiKeyInputDto inputDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await HandleInvalidModelState(cancellationToken).NoSync();
            return;
        }

        await HandleAILensCodeCompletionPromptAsync(inputDto, apiKey: inputDto.ApiKey, countTokens: false, cancellationToken).NoSync();
    }

    private async Task<string> PromptAndStreamChatCompletion(
        ChatCompletionsOptions chatCompletionsOptions,
        string apiKey = null,
        bool bufferContent = true,
        CancellationToken cancellationToken = default)
    {
        var isCustomApiKey = !string.IsNullOrWhiteSpace(apiKey);
        Azure.AI.OpenAI.StreamingResponse<StreamingChatCompletionsUpdate> completionResponse;

        if (isCustomApiKey)
        {
            completionResponse = await openAIChatCompletionService.PromptChatCompletionWithApiKeyWithStreaminAsync(
                    chatCompletionsOptions,
                    apiKey,
                    cancellationToken);
        }
        else
        {
            completionResponse = await openAIChatCompletionService.PromptChatCompletionWithStreamingAsync(
                    chatCompletionsOptions,
                    cancellationToken);
        }

        return await Response.StreamChatCompletionsAsync(
            completionResponse,
            bufferContent: bufferContent,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCodeCompletionPromptAsync(
        IChatCompletionInputDto inputDto,
        string apiKey = null,
        bool countTokens = true,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var allMessages = new List<ChatMessage>
        {
            new(ChatRole.System, AIInstructions.GlobalCodingInstruction),
        };

        var chatId = inputDto.ChatId;
        if (chatId.HasValue)
        {
            var contextMessages = await aIContextService.GetContextMessages(chatId.Value);
            var openAIContextMessages = contextMessages.ToChatMessages();
            allMessages.AddRange(openAIContextMessages);
        }

        if (!string.IsNullOrWhiteSpace(inputDto.ContextCode))
        {
            allMessages.AddRange(
                new ChatMessage[] {
                    new(ChatRole.System, "User provides code snippet and you should consider it as a context for your responses."),
                    new(ChatRole.User, inputDto.ContextCode)
                });
        }

        if (!string.IsNullOrWhiteSpace(inputDto.FileExtension))
        {
            var instruction = "The following is a programming language extension of provided code snippet: " + inputDto.FileExtension;
            var message = new ChatMessage(ChatRole.User, instruction);
            allMessages.Add(message);
        }

        // Merge system messages with user prompt
        var messages = new List<ChatMessage>(allMessages)
        {
            new(ChatRole.User, inputDto.Prompt)
        };

        var aiModel = inputDto.AiModel;
        var maxTokens = inputDto.MaxTokens;

        // Main validation of user prompt
        var inputTokens = await openAIPromptValidationService.ValidateUserOpenAIPromptAsync(
            messages,
            aiModel,
            maxTokens,
            userId,
            isPrompt: true,
            cancellationToken);

        var chatCompletionsOptions = new ChatCompletionsOptions(aiModel, messages)
        {
            MaxTokens = maxTokens,
            ChoiceCount = 1,
        };

        var outputContent = await PromptAndStreamChatCompletion(chatCompletionsOptions, apiKey, bufferContent: countTokens, cancellationToken);

        if (chatId != null)
        {
            var userMessage = new AIContextMessage(inputDto.Prompt, ContextChatRole.User);
            var assistantMessage = new AIContextMessage(outputContent, ContextChatRole.Assistant);

            var newCtxMessages = new List<AIContextMessage> { userMessage, assistantMessage };
            aIContextService.AddMessagesToContextAsBackgroundTaskAsync(chatId.Value, newCtxMessages);
        }

        if (countTokens)
        {
            var outputTokens = tokenizerService.CountTokensFromString(aiModel, outputContent);
            var totalTokens = inputTokens + outputTokens;

            HttpContext.AddAIModelTokensUsageHeaders(totalTokens, aiModel);
        }
    }

    private async Task HandleAILensCodeCompletionPromptAsync(
        IAILensCompletionInputDto inputDto,
        string apiKey = null,
        bool countTokens = true,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();

        var lensInstructions = await aiLensService.GetAILensInstructionsAsync(inputDto.AILensId, cancellationToken: cancellationToken);

        var systemMessages = new List<ChatMessage>
        {
            new(ChatRole.System, AIInstructions.GlobalCodingInstruction),
            new(ChatRole.System, lensInstructions.BehaviorInstruction),
            new(ChatRole.System, lensInstructions.ResponseInstruction),
        };

        var messages = new List<ChatMessage>(systemMessages)
        {
            new(ChatRole.User, inputDto.ContextCode),
        };

        var aiModel = inputDto.AiModel;
        var maxTokens = inputDto.MaxTokens;

        // Main validation of user prompt
        var inputTokens = await openAIPromptValidationService.ValidateUserOpenAIPromptAsync(
            messages,
            aiModel,
            maxTokens,
            userId,
            isPrompt: true,
            cancellationToken);

        var chatCompletionsOptions = new ChatCompletionsOptions(aiModel, messages)
        {
            MaxTokens = maxTokens,
            ChoiceCount = 1,
        };

        var outputContent = await PromptAndStreamChatCompletion(chatCompletionsOptions, apiKey, bufferContent: countTokens, cancellationToken);
        if (inputDto.ChatId != null)
        {
            var userMessage = new AIContextMessage(inputDto.ContextCode, ContextChatRole.User);
            var assistantMessage = new AIContextMessage(outputContent, ContextChatRole.Assistant);

            var newCtxMessages = new List<AIContextMessage> { userMessage, assistantMessage };
            aIContextService.AddMessagesToContextAsBackgroundTaskAsync(inputDto.ChatId.Value, newCtxMessages);
        }

        if (countTokens)
        {
            var outputTokens = tokenizerService.CountTokensFromString(aiModel, outputContent);
            var totalTokens = inputTokens + outputTokens;

            HttpContext.AddAIModelTokensUsageHeaders(totalTokens, aiModel);
        }
    }

    private async Task<ICollection<AITestCaseDto>> GenerateUnitTestCasesAsync(
        IGenerateUnitTestCasesInputDto inputDto,
        string model,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, AIInstructions.GlobalBehaviorInstruction),
            new(ChatRole.System, AIInstructions.GenerateUnitTestCasesSystemInstructions),
            new(ChatRole.User, AIInstructions.GenerateUnitTestCasesResponseInstructions),
        };

        if (!string.IsNullOrWhiteSpace(inputDto.FileExtension))
        {
            var instruction = "The following is a programming language extension of provided code snippet: " + inputDto.FileExtension;
            var message = new ChatMessage(ChatRole.System, instruction);
            messages.Add(message);
        }

        var userPromptBuilder = new StringBuilder("Write EXACTLY ");

        if (inputDto.TotalPositiveTestCases > 0)
        {
            userPromptBuilder.Append($" {inputDto.TotalPositiveTestCases} positive");
        }

        if (inputDto.TotalNegativeTestCases > 0)
        {
            userPromptBuilder.Append($" {inputDto.TotalNegativeTestCases} negative");
        }

        if (inputDto.TotalEdgeCaseTestCases > 0)
        {
            userPromptBuilder.Append($" {inputDto.TotalEdgeCaseTestCases} edge case");
        }

        userPromptBuilder.Append(" test cases for the provided method! Code: ");
        userPromptBuilder.Append(inputDto.Code);
        userPromptBuilder.AppendLine();

        if (!string.IsNullOrWhiteSpace(inputDto.TestsCode))
        {
            var instruction = "This is existing code for unit tests. You should consider it as a context for your responses: " + inputDto.TestsCode;
            userPromptBuilder.AppendLine(instruction);
        }

        var userPrompt = userPromptBuilder.ToString();
        messages.Add(new ChatMessage(ChatRole.User, userPrompt));

        var userId = User.GetUserId();

        // Main validation of user prompt
        await openAIPromptValidationService.ValidateUserOpenAIPromptAsync(
            messages,
            model,
            maxTokens,
            userId,
            isPrompt: false,
            cancellationToken);

        var aiResponse = await openAIChatCompletionService.PromptChatCompletionAsync(
                       new ChatCompletionsOptions(model, messages)
                       {
                           MaxTokens = maxTokens,
                           ChoiceCount = 1,
                           Temperature = (float)0.3,
                           NucleusSamplingFactor = (float)0.8,
                       },
           cancellationToken);

        var totalUsedTokens = aiResponse.Usage.TotalTokens;
        HttpContext.AddAIModelTokensUsageHeaders(totalUsedTokens, model);

        var responseCode = aiResponse.Choices[0].Message.Content;
        responseCode = responseCode.Replace("\n", string.Empty).Replace("```json", string.Empty).Replace("```", string.Empty);

        var testCases = JsonSerializer.Deserialize<List<AITestCaseDto>>(responseCode, SERIAZLIER_OPTIONS);

        testCases.ForEach(x => x.Id = Guid.NewGuid());

        return testCases.OrderBy(x => x.Type).ToList();
    }

    private async Task<AIUnitTestDto> GenerateUnitTestAsync(
        IGenerateUnitTestInputDto inputDto,
        string model,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var systemMessages = new List<ChatMessage>
        {
            new(ChatRole.System, AIInstructions.GlobalBehaviorInstruction),
            new(ChatRole.System, AIInstructions.GenerateUnitTestSystemInstructions),
            new(ChatRole.User, AIInstructions.GenerateUnitTestResponseInstructions)
        };

        if (!string.IsNullOrWhiteSpace(inputDto.FileExtension))
        {
            var instruction = "The following is a programming language extension of provided code snippet: " + inputDto.FileExtension;
            var message = new ChatMessage(ChatRole.System, instruction);
            systemMessages.Add(message);
        }

        var userPromptBuilder = new StringBuilder("Write unit test for provided method and this test case: ");
        userPromptBuilder.Append("Title: ").Append(inputDto.Title).Append(", ");
        userPromptBuilder.Append("When: ").Append(inputDto.When).Append(", ");
        userPromptBuilder.Append("Given: ").Append(inputDto.Given).Append(", ");
        userPromptBuilder.Append("Then: ").Append(inputDto.Then).Append(". ");
        userPromptBuilder.Append("This is a ").Append(inputDto.Type.ToString()).Append(" test case. ");
        userPromptBuilder.Append($"Use {inputDto.TestFramework} framework to write the test.").AppendLine();
        userPromptBuilder.Append("Code: ").Append(inputDto.Code);

        if (!string.IsNullOrWhiteSpace(inputDto.TestsCode))
        {
            var instruction = "This is existing code for unit tests. You should consider it as a context for your responses: " + inputDto.TestsCode;
            userPromptBuilder.AppendLine(instruction);
        }

        var userPrompt = userPromptBuilder.ToString();
        var messages = new List<ChatMessage>(systemMessages)
        {
            new(ChatRole.User, userPrompt),
        };

        var userId = User.GetUserId();

        // Main validation of user prompt
        await openAIPromptValidationService.ValidateUserOpenAIPromptAsync(
            messages,
            model,
            maxTokens,
            userId,
            isPrompt: false,
            cancellationToken);

        var aiResponse = await openAIChatCompletionService.PromptChatCompletionAsync(
             new ChatCompletionsOptions(model, messages)
             {
                 MaxTokens = maxTokens,
                 ChoiceCount = 1,
                 Temperature = (float)0.2,
                 NucleusSamplingFactor = (float)0.8,
             }, cancellationToken);

        var totalUsedTokens = aiResponse.Usage.TotalTokens;
        HttpContext.AddAIModelTokensUsageHeaders(totalUsedTokens, model);

        var responseCode = aiResponse.Choices[0].Message.Content;

        var unitTest = new AIUnitTestDto
        {
            Id = inputDto.Id,
            Title = inputDto.Title,
            Type = inputDto.Type,
            When = inputDto.When,
            Given = inputDto.Given,
            Then = inputDto.Then,
            Code = responseCode,
        };

        return unitTest;
    }
}
