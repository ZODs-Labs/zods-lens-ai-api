using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ZODs.Api.Repository.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCmDbContext(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            services.AddDbContext<ZodsContext>(opt => opt.UseNpgsql(connectionString), ServiceLifetime.Scoped);
        }
    }
}
