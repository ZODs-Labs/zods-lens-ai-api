using ZODs.AI.OpenAI.Constants;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public class ChatCompletionWithApiKeyInputDto
{
    [Required(ErrorMessage = "You must provide your an API key.")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "You must selected AI model.")]
    public string AiModel { get; set; } = OpenAIModels.Gpt4o_Mini;

    public string FileExtension { get; set; } = string.Empty;

    public string ContextCode { get; set; } = string.Empty;

    [Range(10, 4_000, ErrorMessage = "Max tokens must be between 10 and 4,000.")]
    public int MaxTokens { get; set; } = 500;
}