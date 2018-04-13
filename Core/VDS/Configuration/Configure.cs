using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using VDS.BackgroundJobs;
using VDS.Data.Uow;
using VDS.Dependency;
using VDS.Helpers.Extensions;
using VDS.Messaging;
using VDS.Reflection;
using VDS.Threading.BackgrodunWorkers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VDS.Configuration
{
    public class Configure : IConfigure, IDisposable
    {
        public Configure(IServiceCollection services)
        {
            Services = services;

            InitializeProperties();
            AddUnitOfWorkFilters();

            RegisterDefaults();
            
            AddIgnoredTypes();
        }

        public static IConfigure New(IServiceCollection services)
        {
            return new Configure(services);
        }

        public IServiceCollection Services { get; }
        public IStorageConfiguration Storage { get; private set; }
        public IBackgroundJobConfiguration BackgroundJobs { get; private set; }
        public IUnitOfWorkDefaultOptions UnitOfWork { get; private set; }
        public IMessageConfiguration Message { get; private set; }
        public INotificationConfiguration Notification { get; private set; }
        public IValidationConfiguration Validation { get; private set; }
        public string DefaultNameOrConnectionString { get; set; }
        
        public void Initialize()
        {
            RegisterMissingComponent();
            ConfigurationDone();
        }

        private void ConfigurationDone()
        {
            var callbacks = Services.BuildServiceProvider().GetServices<IWantToKnowWhenConfigurationIsDone>();
            callbacks.ForEach(async c => await c.Configured(this));
        }

        private void RegisterDefaults()
        {
            Services.AddSingleton(Storage);
            Services.AddSingleton(UnitOfWork);
            Services.AddSingleton(BackgroundJobs);
            Services.AddSingleton(Validation);
            Services.AddSingleton(Message);
            Services.AddSingleton(Notification);
            Services.AddSingleton<IConfigure>(this);
            Services.AddAssemblyConvention(typeof(IConfigure).Assembly);
            Services.AddAssembliesConvention(AppDomain.CurrentDomain.GetExcutingAssembiles().Except(new []{ typeof(IConfigure).Assembly }));
            Services.AddSingleton<ITypeFinder, TypeFinder>();
            Services.AddSingleton<IAssemblyFinder, AssemblyFinder>();
            Services.AddScoped<SingleInstanceFactory>(p => p.GetRequiredService);
            Services.AddScoped<MultiInstanceFactory>(p => p.GetRequiredServices);
            Services.AddMediatR(AppDomain.CurrentDomain.GetExcutingAssembiles());
        }

        private void RegisterMissingComponent()
        {
            if (!Services.IsRegistered(typeof(IGuidGenerator)))
            {
                Services.AddSingleton<IGuidGenerator>(SequentialGuidGenerator.Instance);
            }

            Services.AddIfNot<IUnitOfWork, NullUnitOfWork>(ServiceLifetime.Transient);
            Services.AddIfNot<IUnitOfWorkFilterExecuter, NullUnitOfWorkFilterExecuter>(ServiceLifetime.Singleton);

            if (BackgroundJobs.IsJobExecutionEnabled)
            {
                var memoryBackgroundStore = new InMemoryBackgroundJobStore();
                Services.AddSingletonIfNot<IBackgroundJobStore>(memoryBackgroundStore);
            }
            else
            {
                Services.Replace(ServiceDescriptor.Singleton<IBackgroundJobStore, NullBackgroundJobStore>());
            }
        }

        private void InitializeProperties()
        {
            Storage = new StorageConfiguration(this);
            BackgroundJobs = new BackgroundJobConfiguration(this);
            Message = new MessageConfiguration(this);
            Notification = new NotificationConfiguration(this);
            Validation = new ValidationConfiguration(this);
            UnitOfWork = new UnitOfWorkDefaultOptions();
        }

        private void AddUnitOfWorkFilters()
        {
            UnitOfWork.RegisterFilter(DataFilters.SoftDelete, true);
        }

        private void AddIgnoredTypes()
        {
            var commonIgnoredTypes = new[]
            {
                typeof(Stream),
                typeof(Expression)
            };

            foreach (var ignoredType in commonIgnoredTypes)
            {
                Validation.IgnoredTypes.AddIfNotContains(ignoredType);
            }

            var validationIgnoredTypes = new[] { typeof(Type) };
            foreach (var ignoredType in validationIgnoredTypes)
            {
                Validation.IgnoredTypes.AddIfNotContains(ignoredType);
            }
        }

        public void Dispose()
        {
            if (BackgroundJobs.IsJobExecutionEnabled)
            {
                Services.BuildServiceProvider().GetService<IBackgroundWorkerManager>().WaitToStop();
            }
        }
    }
}