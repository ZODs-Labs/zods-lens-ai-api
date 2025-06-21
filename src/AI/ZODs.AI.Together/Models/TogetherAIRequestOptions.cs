using Azure.Core;
using ZODs.AI.Common;
using ZODs.AI.Common.Extensions;
using ZODs.AI.Common.Interfaces;

namespace ZODs.AI.Together;

public sealed partial class TogetherAIRequestOptions : IAIRequestOptions
{
    public string AiModel { get; set; } = string.Empty;

    public int MaxTokens { get; set; } = 512;

    public decimal Temperature { get; set; } = 0.8M;

    public decimal TopP { get; set; } = 0.7M;

    public int TopK { get; set; } = 50;

    public decimal RepetitionPenalty { get; set; } = 1M;

    public bool StreamTokens { get; set; }

    public ICollection<string> StopSequences { get; set; } = [];

    public ICollection<IAIChatMessage> ChatMessages { get; set; } = [];

    public RequestContent ToRequestContent()
    {
        var content = new Utf8JsonRequestContent();
        content.JsonWriter.WriteObjectValue(this);
        return content;
    }
}
