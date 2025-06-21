using ZODs.AI.Common.InputDtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public sealed class Gpt3ChatCompletionPromptInputDto : Gpt3CompletionInputDto, IChatCompletionInputDto
{
    [Required(ErrorMessage = "You must provide prompt.")]
    public string Prompt { get; set; } = string.Empty;

    public Guid? ChatId { get; set; }
}