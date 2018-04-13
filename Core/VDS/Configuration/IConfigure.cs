using VDS.BackgroundJobs;
using VDS.Data.Uow;
using VDS.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.Configuration
{
    /// <summary>
    /// Define the configuration
    /// </summary>
    public interface IConfigure
    {
        /// <summary>
        /// Dependency injection
        /// </summary>
        IServiceCollection Services { get; }
        /// <summary>
        /// Used to configure storage.
        /// </summary>
        IStorageConfiguration Storage { get; }

        /// <summary>
        /// Used to configure background jobs.
        /// </summary>
        IBackgroundJobConfiguration BackgroundJobs { get; }

        /// <summary>
        /// Used to configure unit of work default.
        /// </summary>
        IUnitOfWorkDefaultOptions UnitOfWork { get; }

        /// <summary>
        /// Used to configure message bus.
        /// </summary>
        IMessageConfiguration Message { get; }

        /// <summary>
        /// Used to configure notification.
        /// </summary>
        INotificationConfiguration Notification { get; }

        /// <summary>
        /// Used to configure validation.
        /// </summary>
        IValidationConfiguration Validation { get; }

        /// <summary>
        /// Gets/sets default connection string used by ORM module.
        /// It can be name of a connection string in application's config file or can be full connection string.
        /// </summary>
        string DefaultNameOrConnectionString { get; set; }
        
        /// <summary>
        /// Initializes the system
        /// </summary>
        void Initialize();
    }
}