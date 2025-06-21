using ZODs.AI.Common.InputDtos.Interfaces;
using ZODs.AI.OpenAI.Constants;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public class Gpt3CompletionInputDto : IBaseCodeCompletionPromptInputDto
{
    [Required(ErrorMessage = "You must provide prompt.")]
    [AllowedValues(OpenAIModels.Gpt4o_Mini, ErrorMessage = "Invalid model.")]
    public string AiModel { get; set; } = OpenAIModels.Gpt4o_Mini;

    public string FileExtension { get; set; } = string.Empty;

    public string ContextCode { get; set; } = string.Empty;

    [Range(10, 4_000, ErrorMessage = "Max tokens must be between 10 and 4,000.")]
    public int MaxTokens { get; set; } = 500;

}