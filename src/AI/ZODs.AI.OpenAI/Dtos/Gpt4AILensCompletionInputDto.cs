using ZODs.AI.Common.InputDtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public class Gpt4AILensCompletionInputDto : GPT4CompletionInputDto, IAILensCompletionInputDto
{
    [Required]
    public Guid AILensId { get; set; }

    public Guid? ChatId { get; set; }
}
