using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.Repositories;
using ZODs.Api.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ZODs.Api.Repository.Extensions
{
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Add Repositories to the DI container.
        /// </summary>
        /// <param name="services">Instance of services collection.</param>
        /// <returns>The same instance of services collection.</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services.AddScoped<ISnippetsRepository, SnippetsRepository>()
                           .AddScoped<IWorkspacesRepository, WorkspacesRepository>()
                           .AddScoped<IUsersRepository, UsersRepository>()
                           .AddScoped<IWorkspaceMemberInvitesRepository, WorkspaceMemberInvitesRepository>()
                           .AddScoped<IDataSeedRepository, DataSeedRepository>()
                           .AddScoped<IPricingPlanRepository, PricingPlanRepository>()
                           .AddScoped<IPricingPlanFeaturesRepository, PricingPlanFeaturesRepository>()
                           .AddScoped<ISnippetTriggerPrefixesRepository, SnippetTriggerPrefixesRepository>()
                           .AddScoped<IAILensRepository, AILensRepository>()
                           .AddScoped<IUserAICreditBalanceRepository, UserAICreditBalanceRepository>()
                           .AddScoped<IRequestLogsRepository, RequestLogsRepository>();
        }

        /// <summary>
        /// Registers the unit of work given context as a service in the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of the db context.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        /// <remarks>
        /// This method only support one db context, if been called more than once, will throw exception.
        /// </remarks>
        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            return services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
        }
    }
}
