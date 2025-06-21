using Microsoft.Extensions.DependencyInjection;

namespace ZODs.AI.Google.Extensions;

public static class GoogleAIExtensions
{
    /// <summary>
    /// Add Gemini AI client to the service collection.
    /// </summary>
    /// <param name="services">An instance of <see cref="IServiceCollection"/>.</param>
    /// <returns>Service collection with the Gemini AI client added.</returns>
    public static IServiceCollection AddGeminiAIClient(this IServiceCollection services)
    {
        return services.AddSingleton<IGeminiAIClient, GeminiAIClient>();
    }
}
