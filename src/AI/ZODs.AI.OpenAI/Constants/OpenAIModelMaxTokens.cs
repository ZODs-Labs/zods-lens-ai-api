namespace ZODs.AI.OpenAI.Constants;

public static class OpenAIModelMaxTokens
{
    public static readonly Dictionary<string, int> ModelMaxTokens = new()
    {
        { "gpt-4o-mini", 16_000 },
        { "gpt-4o", 4_097 },
    };
}