using ZODs.AI.Common.InputDtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.Google.InputDtos;

public sealed class GeminiProChatCompletionInputDto : IChatCompletionInputDto
{
    [MaxLength(10_000)]
    public string? Prompt { get; set; }

    public string AiModel { get; set; } = string.Empty;

    public string FileExtension { get; set; } = string.Empty;

    public string? ContextCode { get; set; } = string.Empty;

    [Range(10, 2_000, ErrorMessage = "Max tokens must be between 10 and 2,000.")]
    public int MaxTokens { get; set; }

    public Guid? ChatId { get; set; }
}
