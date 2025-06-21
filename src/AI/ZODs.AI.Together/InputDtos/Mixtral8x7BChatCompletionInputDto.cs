using ZODs.AI.Common.InputDtos.Interfaces;
using ZODs.AI.Together.Constants;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.Together.InputDtos;

public sealed class Mixtral8x7BChatCompletionInputDto : IChatCompletionInputDto
{
    [Required]
    [MaxLength(10_000, ErrorMessage = "The message you submitted was too long. Please try to shorten it and submit again.")]
    public string? Prompt { get; set; }

    [AllowedValues(TogetherAIModels.Mixtral8x7B, ErrorMessage = "The model you selected is not supported.")]
    public string AiModel { get; set; } = string.Empty;

    public string FileExtension { get; set; } = string.Empty;

    public string ContextCode { get; set; } = string.Empty;

    [Range(10, 2_000, ErrorMessage = "Max tokens must be between 10 and 2,000.")]
    public int MaxTokens { get; set; }

    public Guid? ChatId { get; set; }
}
