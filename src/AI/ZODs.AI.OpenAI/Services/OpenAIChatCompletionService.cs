using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZODs.AI.OpenAI.Configuration;
using ZODs.AI.OpenAI.Interfaces;
using ZODs.Common.Exceptions;

namespace ZODs.AI.OpenAI.Services;

public sealed class OpenAIChatCompletionService : IOpenAIChatCompletionService
{
    private readonly OpenAIConfiguration openAIConfiguration;
    private readonly OpenAIClient client;
    private readonly ILogger<OpenAIChatCompletionService> logger;

    public OpenAIChatCompletionService(IOptions<OpenAIConfiguration> openAIConfiguration, ILogger<OpenAIChatCompletionService> logger)
    {
        this.openAIConfiguration = openAIConfiguration.Value;
        this.openAIConfiguration.ValidateConfiguration();

        client = new OpenAIClient(this.openAIConfiguration.ApiKey);
        this.logger = logger;
    }

    public async Task<StreamingResponse<StreamingChatCompletionsUpdate>> PromptChatCompletionWithStreamingAsync(
        ChatCompletionsOptions chatCompletionsOptions,
        CancellationToken cancellationToken)
    {
        StreamingResponse<StreamingChatCompletionsUpdate> response = await client.GetChatCompletionsStreamingAsync(
            chatCompletionsOptions,
            cancellationToken);

        return response;
    }

    public async Task<StreamingResponse<StreamingChatCompletionsUpdate>> PromptChatCompletionWithApiKeyWithStreaminAsync(
        ChatCompletionsOptions chatCompletionsOptions,
            string apiKey,
            CancellationToken cancellationToken)
    {
        OpenAIClient client = CreateClient(apiKey);

        try
        {
            StreamingResponse<StreamingChatCompletionsUpdate> response = await client.GetChatCompletionsStreamingAsync(
                chatCompletionsOptions,
                cancellationToken);

            return response;
        }
        catch (RequestFailedException reqFailedEx) when (reqFailedEx.Message.Contains("Incorrect API key provided"))
        {
            logger.LogError("OpenAI Custom Api Key - Request failed exception: {msg}", reqFailedEx.Message);
            throw new ClientSideException("Invalid OpenAI API key.");
        }
    }

    public async Task<ChatCompletions> PromptChatCompletionAsync(
        ChatCompletionsOptions chatCompletionsOptions,
        CancellationToken cancellationToken)
    {
        var response = await client.GetChatCompletionsAsync(
                chatCompletionsOptions,
                cancellationToken);

        return response.Value;
    }

    private static OpenAIClient CreateClient(string apiKey) => new(apiKey);
}