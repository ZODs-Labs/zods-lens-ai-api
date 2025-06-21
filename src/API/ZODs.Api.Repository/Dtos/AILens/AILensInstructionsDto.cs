namespace ZODs.Api.Repository.Dtos.AILens;

public sealed class AILensInstructionsDto
{
    public string BehaviorInstruction { get; set; } = string.Empty;

    public string ResponseInstruction { get; set; } = string.Empty;
}