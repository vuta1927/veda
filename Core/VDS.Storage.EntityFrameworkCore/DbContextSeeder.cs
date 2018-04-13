using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.Dependency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VDS.Storage.EntityFrameworkCore
{
    public class DbContextSeeder : ITransientDependency
    {
        private readonly IEnumerable<IDbContextSeed> _contextSeeds;
        private readonly ILoggerFactory _logger;
        private readonly IServiceProvider _services;

        public DbContextSeeder(IEnumerable<IDbContextSeed> contextSeeds, ILoggerFactory logger, IServiceProvider services)
        {
            _contextSeeds = contextSeeds;
            _logger = logger;
            _services = services;
        }

        public async Task BeginSeedAsync()
        {
            foreach (var contextSeed in _contextSeeds)
            {
                var logger = _logger.CreateLogger(contextSeed.GetType());

                if (!(_services.GetService(contextSeed.ContextType) is DbContext context))
                {
                    continue;
                }

                try
                {
                    logger.LogInformation($"Migrating database associated with context {contextSeed.ContextType.Name}");

                    await context.Database.MigrateAsync();
                    await contextSeed.SeedAsync();

                    logger.LogInformation($"Migrated database associated with context {contextSeed.ContextType.Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred while migrating the database used on context {contextSeed.ContextType.Name}");
                }
            }
        }
    }
}