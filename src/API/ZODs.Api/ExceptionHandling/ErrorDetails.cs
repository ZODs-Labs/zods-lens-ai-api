using System.Text.Json;

namespace ZODs.Api.ExceptionHandling;

public class ErrorDetails
{
    private static JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public int StatusCode { get; set; }

    public string Message { get; set; }

    public override string ToString() => JsonSerializer.Serialize(
            this,
            serializerOptions);
}
