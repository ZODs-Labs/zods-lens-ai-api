using Azure.Core;
using ZODs.AI.Common;
using ZODs.AI.Common.Extensions;
using ZODs.AI.Common.Interfaces;

namespace ZODs.AI.Google.Models;

public sealed partial class GeminiAIRequestOptions : IAIRequestOptions
{
    public string AiModel
    {
        get; set;
    } = string.Empty;

    public int MaxTokens
    {
        get; set;
    }

    public decimal Temperature
    {
        get; set;
    }

    public ICollection<IAIChatMessage> ChatMessages { get; set; } = [];

    public ICollection<string> StopSequences
    {
        get; set;
    } = [];

    public RequestContent ToRequestContent()
    {
        var content = new Utf8JsonRequestContent();
        content.JsonWriter.WriteObjectValue(this);
        return content;
    }
}
