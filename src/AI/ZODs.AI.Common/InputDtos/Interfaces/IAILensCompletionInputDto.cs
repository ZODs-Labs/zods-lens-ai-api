namespace ZODs.AI.Common.InputDtos.Interfaces;

public interface IAILensCompletionInputDto : IBaseCodeCompletionPromptInputDto
{
    Guid AILensId { get; }

    Guid? ChatId { get; }
}