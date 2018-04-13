//using System;
//using Castle.Windsor;
//using VDS.Configuration;
//using VDS.Messaging.NServiceBus.Messages;
//using VDS.Messaging.NServiceBus.Persistence;
//using VDS.Messaging.NServiceBus.Transport;
//using VDS.Messaging.NServiceBus.UnitOfWork;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using NServiceBus;
//using NServiceBus.Logging;
//using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
//
//namespace VDS.Messaging.NServiceBus
//{
//    public static class ConfigurationExtensions
//    {
//        public static IConfigure UseNServiceBus(this IMessageConfiguration messageConfiguration,
//            Action<NServiceBusConfig> configAction)
//        {
//            var config = new NServiceBusConfig();
//            configAction(config);
//
//            messageConfiguration.Configure.Services.AddSingleton(config);
//
//            // Create NServiceBus Endpoint Configuration
//            var endpointConfiguration = config.EndpointConfiguration = new EndpointConfiguration(config.EndpointName);
//
//            // Configure Logging
//            if (!string.IsNullOrEmpty(config.LogDirectory))
//            {
//                var defaultFactory = LogManager.Use<DefaultFactory>();
//                var logDirPath = config.LogDirectory;
//                defaultFactory.Directory(logDirPath);
//                if (config.LogLevel != null)
//                {
//                    defaultFactory.Level(config.LogLevel.Value);
//                }
//            }
//
//            // Endpoint Level Config
//            endpointConfiguration.PurgeOnStartup(false);
//            endpointConfiguration.SendFailedMessagesTo(config.ErrorQueue);
//            endpointConfiguration.AuditProcessedMessagesTo(config.AuditQueue);
//
//            // Transport
//            if (!config.DoNotUseDefaultTransport)
//                endpointConfiguration.ConfigureNServiceBusDefaultTransport(messageConfiguration);
//
//            // MaxConcurrencyLevel
//            if (config.MaximumConcurrencyLevel.HasValue)
//                endpointConfiguration.LimitMessageProcessingConcurrencyTo(config.MaximumConcurrencyLevel.Value);
//
//            // Persistence
//            if (!config.DoNotUseDefaultPersistence)
//                endpointConfiguration.ConfigureNServiceBusDefaultPersistence(messageConfiguration);
//
//            // Outbox
//            if (config.UseOutbox)
//                endpointConfiguration.EnableOutbox();
//
//            // Unobtrusive Message Box
//            endpointConfiguration.UseNServiceBusMessageConventions();
//
//            // Container
//            endpointConfiguration.UseContainer<WindsorContainer>(
//                customizations: customizations =>
//                {
//                    customizations.U
//                });
//
//            // Recoverability
//            endpointConfiguration.Recoverability()
//                .Immediate(customizations: immediate =>
//                {
//                    immediate.NumberOfRetries(config.ImmediateRetries);
//                })
//                .Delayed(customizations: delayed =>
//                {
//                    delayed.NumberOfRetries(config.DelayedRetries);
//                    delayed.TimeIncrease(TimeSpan.FromSeconds(config.DelayedRetriesTimeIncreaseInSeconds));
//                });
//
//            if (config.UseEntityFrameworkUnitOfWork)
//            {
//                endpointConfiguration.Pipeline.Register(typeof(NServiceBusUnitOfWork), typeof(NServiceBusUnitOfWork).Name);
//            }
//
//            var logger = messageConfiguration.Configure.Services.BuildServiceProvider().GetService<ILoggerFactory>()
//                .CreateLogger("Messaging.NServiceBus");
//
//            logger.LogInformation("NServiceBus endpoint {0} is starting...", config.EndpointName);
//
//            endpointConfiguration.EnableInstallers();
//            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
//
//            messageConfiguration.Configure.Services.AddSingleton(endpointInstance);
//
//            logger.LogInformation("NServiceBus endpoint {0} started successfully", config.EndpointName);
//
//            return messageConfiguration.Configure;
//        }
//    }
//}