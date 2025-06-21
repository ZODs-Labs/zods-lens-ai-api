namespace ZODs.AI.Google.Models;

public class GeminiPart
{
    public string Text { get; set; } = string.Empty;
}

public class GeminiContent
{
    public GeminiPart[] Parts { get; set; } = [];
}

public class GeminiRequest
{
    public GeminiContent[] Contents { get; set; } = [];
}
