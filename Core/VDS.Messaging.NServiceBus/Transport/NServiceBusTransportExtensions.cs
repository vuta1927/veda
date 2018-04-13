using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Transport.SQLServer;

namespace VDS.Messaging.NServiceBus.Transport
{
    public static class NServiceBusTransportExtensions
    {
        public static TransportExtensions<SqlServerTransport> ConfigureNServiceBusDefaultTransport(
            this EndpointConfiguration endpointConfiguration, IMessageConfiguration messageConfiguration)
        {
            var config = messageConfiguration.Configure.Services.BuildServiceProvider().GetService<NServiceBusConfig>();

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();

            // Default Peek Delay
            transport.WithPeekDelay(config.TransportPeekDelay);

            // Schemas
            transport.UseSchemaForQueue(config.AuditQueue, "dbo");
            transport.UseSchemaForQueue(config.ErrorQueue, "dbo");
            transport.UseCatalogForEndpoint(config.EndpointName, config.EndpointDatabaseSchema);

            if (!string.IsNullOrEmpty(config.TransportConnectionString))
                transport.ConnectionString(config.TransportConnectionString);

            if (config.TransportTransactionMode.HasValue)
                transport.Transactions(config.TransportTransactionMode.Value);

            return transport;
        }
    }
}