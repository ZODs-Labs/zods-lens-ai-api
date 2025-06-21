namespace ZODs.AI.Common.InputDtos.Interfaces;

public interface IBaseCodeCompletionPromptInputDto
{
    string AiModel { get; }

    string FileExtension { get; }

    string ContextCode { get; }

    int MaxTokens { get; }
}