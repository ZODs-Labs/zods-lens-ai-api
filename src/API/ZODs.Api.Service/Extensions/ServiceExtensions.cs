using ZODs.Api.Service.Managers;
using ZODs.Api.Service.Services;
using ZODs.Api.Service.Strategies.FeatureLimitationSync;
using ZODs.Api.Service.Validation;
using ZODs.Api.Service.Validation.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ZODs.Api.Service.Factories;
using ZODs.Api.Service.Factories.Interfaces;
using ZODs.Api.Service.Strategies.UserSubscriptionTransition;

namespace ZODs.Api.Service.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Register all services.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCmServices(this IServiceCollection services)
    {
        services.AddValidationServices();

        return services.AddScoped<ISnippetsService, SnippetsService>()
                       .AddScoped<IUsersService, UsersService>()
                       .AddScoped<IWorkspaceService, WorkspaceService>()
                       .AddScoped<IPricingPlanService, PricingPlanService>()
                       .AddScoped<ISnippetTriggerPrefixService, SnippetTriggerPrefixService>()
                       .AddScoped<IDataSeedService, DataSeedService>()
                       .AddScoped<IFeatureLimitationService, FeatureLimitationService>()
                       .AddScoped<IUserInfoService, UserInfoService>()
                       .AddScoped<IAILensService, AILensService>()
                       .AddScoped<IUserAICreditBalanceService, UserAICreditBalanceService>()
                       .AddScoped<IRequestLogsService, RequestLogsService>();
    }

    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        return services.AddScoped<ISnippetValidationService, SnippetValidationService>()
                       .AddScoped<IWorkspaceValidationService, WorkspaceValidationService>()
                       .AddScoped<IOpenAIPromptValidationService, OpenAIPromptValidationService>();
    }

    public static IServiceCollection AddFeatureLimitationSync(this IServiceCollection services)
    {
        services.AddScoped<IFeatureLimitationStrategyFactory, FeatureLimitationStrategyFactory>()
                .AddScoped<MaxPersonalSnippetsLimitationStrategy>()
                .AddScoped<MaxWorkspacesLimitationStrategy>()
                .AddScoped<MaxWorkspaceInvitesLimitationStrategy>()
                .AddScoped<MaxWorkspaceSnippetsLimitationStrategy>()
                .AddScoped<MaxPersonalSnippetPrefixesLimitationStrategy>()
                .AddScoped<MaxWorkspaceSnippetPrefixesLimitationStrategy>()
                .AddScoped<MaxAILensesLimitationStrategy>();

        return services;
    }

    public static IServiceCollection AddSubscriptionTransitionServices(this IServiceCollection services)
    {
        services.AddScoped<IUserSubscriptionManager, UserSubscriptionManager>()
                .AddScoped<IUserSubscriptionTransitionStrategyFactory, UserSubscriptionTransitionStrategyFactory>()
                .AddScoped<PastDueSubscriptionTransitionStrategy>()
                .AddScoped<PastDueSubscriptionTransitionStrategy>()
                .AddScoped<UnpaidSubscriptionTransitionStrategy>()
                .AddScoped<CancelledSubscriptionTransitionStrategy>()
                .AddScoped<ExpiredSubscriptionTransitionStrategy>();

        return services;
    }

    public static IServiceCollection AddFeatureLimitationSyncManager(this IServiceCollection services)
    {
        services.AddScoped<IFeatureLimitationSyncManager, FeatureLimitationSyncManager>();

        return services;
    }
}
