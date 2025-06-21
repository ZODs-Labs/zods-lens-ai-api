namespace ZODs.AI.Together;

public sealed class TogetherAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new ArgumentException("Together API key is required.", nameof(ApiKey));
        }

        if (string.IsNullOrEmpty(Endpoint))
        {
            throw new ArgumentException("Together API endpoint is required.", nameof(Endpoint));
        }
    }
}
