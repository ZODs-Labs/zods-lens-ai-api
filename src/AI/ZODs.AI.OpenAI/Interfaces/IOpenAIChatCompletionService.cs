using Azure.AI.OpenAI;

namespace ZODs.AI.OpenAI.Interfaces;

public interface IOpenAIChatCompletionService
{
    Task<StreamingResponse<StreamingChatCompletionsUpdate>> PromptChatCompletionWithStreamingAsync(
        ChatCompletionsOptions chatCompletionsOptions,
        CancellationToken cancellationToken);
    Task<StreamingResponse<StreamingChatCompletionsUpdate>> PromptChatCompletionWithApiKeyWithStreaminAsync(ChatCompletionsOptions chatCompletionsOptions, string apiKey, CancellationToken cancellationToken);
    Task<ChatCompletions> PromptChatCompletionAsync(ChatCompletionsOptions chatCompletionsOptions, CancellationToken cancellationToken);
}