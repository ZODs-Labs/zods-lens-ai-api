using ZODs.AI.Common;
using System.Text.Json;

namespace ZODs.AI.Google.Models;

public class GeminiMessageResponse
{
    public List<Candidate> Candidates { get; set; } = [];
}

public class Candidate
{
    public Content Content { get; set; } = new();

    public string FinishReason { get; set; } = string.Empty;

    public int Index { get; set; }
}

public class Content
{
    public List<Part> Parts { get; set; } = [];

    public string Role { get; set; } = string.Empty;
}

public class Part
{
    public string Text { get; set; } = string.Empty;
}

public sealed class GeminiAICompletion : IAICompletion
{
    public IReadOnlyList<Candidate> Candidates { get; init; } = [];

    public static GeminiAICompletion DeserializeCompletions(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return new();
        }

        IReadOnlyList<Candidate> candidates = [];
        foreach (var prop in element.EnumerateObject())
        {
            if (prop.NameEquals("candidates"u8))
            {
                candidates = prop.Value.EnumerateArray().Select(candidate =>
                {
                    var content = candidate.GetProperty("content"u8);
                    var parts = content.GetProperty("parts"u8).EnumerateArray().Select(part => new Part
                    {
                        Text = part.GetProperty("text"u8).GetString()!,
                    }).ToList();

                    return new Candidate
                    {
                        Content = new Content
                        {
                            Parts = parts,
                            Role = content.GetProperty("role"u8).GetString()!,
                        },
                        FinishReason = candidate.GetProperty("finish_reason"u8).GetString()!,
                        Index = candidate.GetProperty("index"u8).GetInt32(),
                    };
                }).ToList();
            }
        }

        return new GeminiAICompletion
        {
            Candidates = candidates,
        };
    }
}
