using Microsoft.Extensions.DependencyInjection;
using ZODs.AI.OpenAI.Interfaces;
using ZODs.AI.OpenAI.Services;

namespace ZODs.AI.OpenAI.Extensions;

public static class OpenAIServiceExtensions
{
    public static IServiceCollection AddOpenAITokenizerService(this IServiceCollection services)
    {
        return services.AddSingleton<ITokenizerService, TokenizerService>();
    }

    public static IServiceCollection AddOpenAIChatCompletionService(this IServiceCollection services)
    {
        return services.AddSingleton<IOpenAIChatCompletionService, OpenAIChatCompletionService>();
    }
}