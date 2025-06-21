using ZODs.AI.Common;
using ZODs.AI.Common.Extensions;
using System.Text.Json;

namespace ZODs.AI.Together;

public sealed partial class TogetherAIRequestOptions : IAIRequestOptions
{
    public void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteStartArray("messages"u8);
        foreach (var message in ChatMessages)
        {
            WriteMessage(writer, message.Role.ToEnumString(), message.Content);
        }

        writer.WriteEndArray();

        writer.WritePropertyName("stream_tokens"u8);
        writer.WriteBooleanValue(StreamTokens);

        writer.WritePropertyName("max_tokens"u8);
        writer.WriteNumberValue(MaxTokens);

        writer.WritePropertyName("temperature"u8);
        writer.WriteNumberValue(Temperature);

        writer.WritePropertyName("top_p"u8);
        writer.WriteNumberValue(TopP);

        writer.WritePropertyName("top_k"u8);
        writer.WriteNumberValue(TopK);

        writer.WritePropertyName("repetition_penalty"u8);
        writer.WriteNumberValue(RepetitionPenalty);

        writer.WritePropertyName("repetitive_penalty"u8);
        writer.WriteNumberValue(RepetitionPenalty);

        if (StopSequences.Count > 0)
        {
            writer.WritePropertyName("stop"u8);
            writer.WriteStringValue(StopSequences.ElementAt(0));
        }

        if (!string.IsNullOrWhiteSpace(AiModel))
        {
            writer.WritePropertyName("model"u8);
            writer.WriteStringValue(AiModel);
        }

        writer.WriteEndObject();
    }

    private static void WriteMessage(Utf8JsonWriter writer, string role, string content)
    {
        writer.WriteStartObject(); // Start of a message object
        writer.WriteString("role", role);
        writer.WriteString("content", content);
        writer.WriteEndObject(); // End of the message object
    }
}
