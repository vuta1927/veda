using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using VDS.Data.Uow;
using VDS.Dependency;
using VDS.Messaging;
using VDS.Storage.EntityFrameworkCore.Configuration;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using ObjectFactory = VDS.Helpers.ObjectFactory;

namespace VDS.Storage.EntityFrameworkCore
{
    public class DefaultDbContextResolver : IDbContextResolver, ITransientDependency
    {
        private static readonly MethodInfo CreateOptionsMethod = typeof(DefaultDbContextResolver).GetMethod("CreateOptions", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextTypeMatcher _dbContextTypeMatcher;

        public DefaultDbContextResolver(
            IServiceProvider serviceProvider,
            IDbContextTypeMatcher dbContextTypeMatcher)
        {
            _serviceProvider = serviceProvider;
            _dbContextTypeMatcher = dbContextTypeMatcher;
        }

        public TDbContext Resolve<TDbContext>(string connectionString, DbConnection existingConnection)
            where TDbContext : DbContext
        {
            var dbContextType = typeof(TDbContext);
            Type concreteType = null;
            var isAbstractDbContext = dbContextType.GetTypeInfo().IsAbstract;
            if (isAbstractDbContext)
            {
                concreteType = _dbContextTypeMatcher.GetConcreteType(dbContextType);
            }

            try
            {
                if (isAbstractDbContext)
                {
                    return (TDbContext) GetDbContextWithParameters(concreteType,
                        CreateOptionsForType(concreteType, connectionString, existingConnection), _serviceProvider.GetService<ICurrentUnitOfWorkProvider>(), _serviceProvider.GetService<IMediator>());
                }

                return (TDbContext)GetDbContextWithParameters(typeof(TDbContext),
                    CreateOptions<TDbContext>(connectionString, existingConnection), _serviceProvider.GetService<ICurrentUnitOfWorkProvider>(), _serviceProvider.GetService<IMediator>());
            }
            catch (Exception ex)
            {
                var hasOptions = isAbstractDbContext ? HasOptions(concreteType) : HasOptions(dbContextType);
                if (!hasOptions)
                {
                    throw new AggregateException($"The parameter name of {dbContextType.Name}'s constructor must be 'options'", ex);
                }

                throw;
            }

            bool HasOptions(Type contextType)
            {
                return contextType.GetConstructors().Any(ctor =>
                {
                    var parameters = ctor.GetParameters();
                    return parameters.Length == 1 && parameters.FirstOrDefault()?.Name == "options";
                });
            }
        }

        private object CreateOptionsForType(Type dbContextType, string connectionString, DbConnection existingConnection)
        {
            return CreateOptionsMethod.MakeGenericMethod(dbContextType).Invoke(this, new object[] { connectionString, existingConnection });
        }

        protected virtual DbContextOptions<TDbContext> CreateOptions<TDbContext>([NotNull] string connectionString, [CanBeNull] DbConnection existingConnection) where TDbContext : DbContext
        {
            if (_serviceProvider.IsAdded<IDbContextConfigurer<TDbContext>>())
            {
                var configuration = new DbContextConfiguration<TDbContext>(connectionString, existingConnection);

                configuration.DbContextOptions.ReplaceService<IEntityMaterializerSource, EntityMaterializerSource>();

                var configurer = _serviceProvider.GetService<IDbContextConfigurer<TDbContext>>();
                configurer.Configure(configuration);
                
                return configuration.DbContextOptions.Options;
            }

            if (_serviceProvider.IsAdded<DbContextOptions<TDbContext>>())
            {
                return _serviceProvider.GetService<DbContextOptions<TDbContext>>();
            }

            throw new AppException($"Could not resolve DbContextOptions for {typeof(TDbContext).AssemblyQualifiedName}.");
        }

        private object GetDbContextWithParameters(Type dbContextType, params object[] parameters)
        {
            return ObjectFactory.CreateInstance(dbContextType, parameters);
        }
    }
}