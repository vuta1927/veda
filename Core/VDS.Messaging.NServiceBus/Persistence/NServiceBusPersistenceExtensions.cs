using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Persistence;

namespace VDS.Messaging.NServiceBus.Persistence
{
    public static class NServiceBusPersistenceExtensions
    {
        public static void ConfigureNServiceBusDefaultPersistence(this EndpointConfiguration endpointConfiguration, IMessageConfiguration configuration)
        {
            var config = configuration.Configure.Services.BuildServiceProvider().GetService<NServiceBusConfig>();

            var nhConfiguration = new NHibernate.Cfg.Configuration
            {
                Properties =
                {
                    ["dialect"] = "NHibernate.Dialect.MsSql2012Dialect",
                    ["connection.provider"] = "NHibernate.Connection.DriverConnectionProvider",
                    ["connection.driver_class"] = "NHibernate.SqlAzure.SqlAzureClientDriver, NHibernate.SqlAzure",
                    ["default_schema"] = config.EndpointDatabaseSchema,
                    ["connection.connection_string"] = config.PersistenceConnectionString
                }
            };

            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            persistence.UseConfiguration(nhConfiguration);
        }
    }

    public class S : IEvent
}