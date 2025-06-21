using ZODs.AI.Common.InputDtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public sealed class ChatCompletionPromptWithApiKeyInputDto : ChatCompletionWithApiKeyInputDto, IChatCompletionInputDto
{
    [Required(ErrorMessage = "You must provide prompt.")]
    public string? Prompt { get; set; }

    public Guid? ChatId { get; set; }
}