using ZODs.AI.Common;
using System.Text.Json;

namespace ZODs.AI.Google.Models;

public sealed partial class GeminiAIRequestOptions : IAIRequestOptions
{
    public void Write(Utf8JsonWriter writer)
    {
        // Start writing the JSON structure
        writer.WriteStartObject(); // Start of root object

        writer.WriteStartArray("contents"); // Start of "contents" array

        writer.WriteStartObject(); // Start of object in "contents" array
        writer.WriteStartArray("parts"); // Start of "parts" array

        writer.WriteStartObject(); // Start of object in "parts" array
        writer.WriteEndObject(); // End of object in "parts" array

        writer.WriteEndArray(); // End of "parts" array
        writer.WriteEndObject(); // End of object in "contents" array

        writer.WriteEndArray(); // End of "contents" array

        writer.WriteEndObject(); // End of root object
    }
}
