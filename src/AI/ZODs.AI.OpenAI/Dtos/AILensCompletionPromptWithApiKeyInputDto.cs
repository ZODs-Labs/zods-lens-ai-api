using ZODs.AI.Common.InputDtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public sealed class AILensCompletionPromptWithApiKeyInputDto : ChatCompletionWithApiKeyInputDto, IAILensCompletionInputDto
{
    [Required(ErrorMessage = "AI Lens not selected.")]
    public Guid AILensId { get; set; }

    public Guid? ChatId { get; set; }
}