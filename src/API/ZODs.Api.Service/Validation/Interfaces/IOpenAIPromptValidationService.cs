using Azure.AI.OpenAI;

namespace ZODs.Api.Service.Validation.Interfaces;

public interface IOpenAIPromptValidationService
{
    Task<int> ValidateUserOpenAIPromptAsync
        (ICollection<ChatMessage> messages, 
        string model,
        int maxOutputTokens, 
        Guid userId,
        bool isPrompt,
        CancellationToken cancellationToken);
}