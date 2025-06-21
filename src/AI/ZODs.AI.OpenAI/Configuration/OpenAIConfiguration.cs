namespace ZODs.AI.OpenAI.Configuration;

public sealed class OpenAIConfiguration
{
    public string ApiKey { get; set; } = string.Empty;

    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentNullException("OpenAI ApiKey is not set.");
        }
    }
}