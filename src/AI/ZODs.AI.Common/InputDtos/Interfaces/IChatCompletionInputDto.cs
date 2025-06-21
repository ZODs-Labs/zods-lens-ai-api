namespace ZODs.AI.Common.InputDtos.Interfaces;

public interface IChatCompletionInputDto : IBaseCodeCompletionPromptInputDto
{
    string? Prompt { get; }

    Guid? ChatId { get; }
}