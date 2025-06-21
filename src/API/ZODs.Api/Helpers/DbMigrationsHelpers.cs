// <copyright file="DbMigrationsHelpers.cs" company="Selfhelp">
// Copyright (c) Selfhelp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ZODs.Api.Repository;

namespace ZODs.Api.Helpers
{
    public static class DbMigrationsHelpers
    {
        /// <summary>
        /// Populates database with seed data.
        /// </summary>
        /// <param name="host">IHost.</param>
        /// <param name="seedData">Determines if seed data should be inserted.</param>
        /// <returns>Task.</returns>
        public static async Task EnsureDatabasesMigrated(IHost host)
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                await EnsureDatabasesMigrated(services);
            }
        }

        public static async Task EnsureDatabasesMigrated(IServiceProvider services)
        {
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ZodsContext>())
                {
                    context.Database.SetCommandTimeout(360);
                    await context.Database.MigrateAsync();
                }
            }
        }
    }
}
