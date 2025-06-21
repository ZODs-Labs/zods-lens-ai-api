using ZODs.AI.Common.InputDtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public sealed class Gpt3AILensCompletionInputDto : Gpt3CompletionInputDto, IAILensCompletionInputDto
{
    [Required(ErrorMessage = "You must provide AI Lens.")]
    public Guid AILensId { get; set; }

    public Guid? ChatId { get; set; }
}