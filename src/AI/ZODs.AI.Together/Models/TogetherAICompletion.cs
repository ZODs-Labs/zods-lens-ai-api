using ZODs.AI.Common;
using System.Text.Json;

namespace ZODs.AI.Together;

public sealed record Choice(
    string Text);

public sealed class TogetherAICompletion : IAICompletion
{
    public IReadOnlyList<Choice> Choices { get; init; } = [];

    public static TogetherAICompletion DeserializeCompletions(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return new();
        }

        IReadOnlyList<Choice> choices = [];
        foreach (var prop in element.EnumerateObject())
        {
            if (prop.NameEquals("choices"u8))
            {
                var choice = prop.Value.EnumerateArray().First();
                choices = [new(choice.GetProperty("text"u8).GetString()!)];
            }
        }

        return new TogetherAICompletion
        {
            Choices = choices,
        };
    }
}
