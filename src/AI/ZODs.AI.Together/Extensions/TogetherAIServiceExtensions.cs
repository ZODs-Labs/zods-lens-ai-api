using Microsoft.Extensions.DependencyInjection;

namespace ZODs.AI.Together;

public static class TogetherAIServiceExtensions
{
    public static IServiceCollection AddTogetherAIClient(
         this IServiceCollection services)
    {
        return services.AddSingleton<ITogetherAIClient, TogetherAIClient>();
    }
}
