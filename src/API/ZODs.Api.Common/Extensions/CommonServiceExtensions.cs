using ZODs.Api.Common.Interfaces;
using ZODs.Api.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using StackExchange.Redis;

namespace ZODs.Api.Common.Extensions
{
    public static class CommonServiceExtensions
    {
        /// <summary>
        /// Add <see cref="IGoogleAuthService"/> to DI container."/>
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>The same instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGoogleAuthService(this IServiceCollection services)
            => services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        /// <summary>
        /// Add <see cref="IEmailService"/> to DI container.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>The same instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAWSSESEmailService(this IServiceCollection services)
            => services.AddTransient<IEmailService, AWSSESService>();

        /// <summary>
        /// Add Mailjet as email service and register <see cref="IEmailService"/> in the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>The same instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMailjetEmailService(this IServiceCollection services)
            => services.AddTransient<IEmailService, MailjetEmailService>();

        public static IServiceCollection AddResendEmailService(
           this IServiceCollection services)
        {
            services.AddHttpClient<ResendClient>();
            services.AddTransient<IResend, ResendClient>();
            services.AddTransient<IEmailService, ResendEmailService>();

            return services;
        }

        /// <summary>
        /// Registers cache service <see cref="ICacheService"/> in the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>The same instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCacheService(this IServiceCollection services, string connectionString)
        {
            return services.AddStackExchangeRedisCache(options => options.Configuration = connectionString)
                           .AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString))
                           .AddSingleton<ICacheService, CacheService>();
        }

        /// <summary>
        /// Add <see cref="IAIContextService"/> to DI container.
        /// </summary>
        /// <param name="services">An instance of <see cref="IServiceCollection"/>.</param>
        /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAIContextService(this IServiceCollection services)
        {
            services.AddTransient<IAIContextService, AIContextService>();
            return services;
        }
    }
}
