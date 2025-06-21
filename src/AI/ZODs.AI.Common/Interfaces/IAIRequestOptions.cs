using Azure.Core;
using ZODs.AI.Common.Interfaces;

namespace ZODs.AI.Common;

public interface IAIRequestOptions : IUtf8JsonSerializable
{
    string AiModel { get; }
    int MaxTokens { get; }
    decimal Temperature { get; }
    ICollection<IAIChatMessage> ChatMessages { get; }
    ICollection<string> StopSequences { get; }

    RequestContent ToRequestContent();
}
