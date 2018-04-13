using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.Dependency.Registration;
using VDS.Helpers.Exception;
using Microsoft.Extensions.DependencyInjection;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace VDS.Dependency
{
    public static class ServiceCollectionExtensions
    {
        public static void AddIfNot<TService, TImplement>(this IServiceCollection services, ServiceLifetime lifetime)
            where TImplement : class, TService
        {
            Throw.IfArgumentNull(services, nameof(services));
            if (!services.IsRegistered(typeof(TService)))
            {
                services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImplement), lifetime));
            }
        }

        public static void AddSingletonIfNot<TService>(this IServiceCollection services, TService implementationInstance)
            where TService : class
        {
            Throw.IfArgumentNull(services, nameof (services));

            if (!services.IsRegistered(typeof(TService)))
            {
                services.AddSingleton(implementationInstance);
            }
        }

        public static void AddIfNot<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            Throw.IfArgumentNull(services, nameof(services));
            if (!services.IsRegistered(typeof(TService)))
            {
                services.Add(new ServiceDescriptor(typeof(TService), factory, lifetime));
            }
        }

        public static bool IsRegistered(this IServiceCollection services, Type serviceType)
        {
            return services.Any(s => s.ServiceType == serviceType);
        }

        public static void AddAssemblyConvention(this IServiceCollection services, Assembly assembly)
        {
            ((IRegistrar)
                Classes
                    .FromAssembly(assembly)
                    .IncludeNonPublicTypes()
                    .BasedOn<ITransientDependency>()
                    .If(type => !type.GetTypeInfo().IsGenericTypeDefinition)
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    .WithService.AllInterfaces()
//                    .WithService.IgnoreInterfaces(new []{typeof(ITransientDependency)})
                    .LifestyleTransient()).Register(services);

            ((IRegistrar)
                Classes
                    .FromAssembly(assembly)
                    .IncludeNonPublicTypes()
                    .BasedOn<IScopeDependency>()
                    .If(type => !type.GetTypeInfo().IsGenericTypeDefinition)
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    .WithService.AllInterfaces()
//                    .WithService.IgnoreInterfaces(new[] { typeof(IScopeDependency) })
                    .LifestyleScoped()).Register(services);

            ((IRegistrar)
                Classes
                    .FromAssembly(assembly)
                    .IncludeNonPublicTypes()
                    .BasedOn<ISingletonDependency>()
                    .If(type => !type.GetTypeInfo().IsGenericTypeDefinition)
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    .WithService.AllInterfaces()
//                    .WithService.IgnoreInterfaces(new[] { typeof(ISingletonDependency) })
                    .LifestyleSingleton()).Register(services);
        }

        public static void AddAssembliesConvention(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                services.AddAssemblyConvention(assembly);
            }
        }

        public static void AddAll<TService>(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ApplyLifetime(Classes.FromAssembly(assembly)
                .IncludeNonPublicTypes()
                .BasedOn<TService>()
                .WithService.AllInterfaces(), lifetime)
                .Register(services);
        }

        private static IRegistrar ApplyLifetime(BasedOnDescriptor basedOnDescriptor, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return basedOnDescriptor.LifestyleSingleton();
                case ServiceLifetime.Scoped:
                    return basedOnDescriptor.LifestyleScoped();
                case ServiceLifetime.Transient:
                    return basedOnDescriptor.LifestyleTransient();
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }

        public static void AddDecorator<TService, TDecorator>(this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TService : class
            where TDecorator : class
        {
            var serviceDescriptor = new ServiceDescriptor(
                typeof(TService),
                provider => Construct<TService, TDecorator>(provider, services), lifetime);
            services.Add(serviceDescriptor);
        }

        private static TDecorator Construct<TService, TDecorator>(IServiceProvider serviceProvider,
            IServiceCollection services)
            where TDecorator : class
            where TService : class
        {
            var type = GetDecoratedType<TService>(services);
            var decoratedConstructor = GetConstructor(type);
            var decoratorConstructor = GetConstructor(typeof(TDecorator));
            var docoratedDependencies = serviceProvider.ResolveConstructorDependencies(
                decoratedConstructor.GetParameters());
            var decoratedService = decoratedConstructor.Invoke(docoratedDependencies.ToArray())
                as TService;
            var decoratorDependencies = serviceProvider.ResolveConstructorDependencies(
                decoratedService,
                decoratorConstructor.GetParameters());
            return decoratorConstructor.Invoke(decoratorDependencies.ToArray()) as TDecorator;
        }

        private static Type GetDecoratedType<TService>(IServiceCollection services)
        {
            if (services.Count(p =>
                p.ServiceType == typeof(TService) &&
                p.ImplementationFactory == null) > 1)
            {
                throw new InvalidOperationException(
                    $"Only one decorated service for interface {nameof(TService)} allowed");
            }

            var nonFactoryDescriptor = services.FirstOrDefault(p =>
                p.ServiceType == typeof(TService) &&
                p.ImplementationFactory == null);
            return nonFactoryDescriptor?.ImplementationType;
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            var availableConstructors = type
                .GetConstructors()
                .Where(c => c.IsPublic)
                .ToList();

            if (availableConstructors.Count != 1)
            {
                throw new InvalidOperationException("Only single constructor types are supported");
            }
            return availableConstructors.First();
        }

        public static IEnumerable<object> GetRequiredServices(this IServiceProvider provider, Type serviceType)
        {
            return (IEnumerable<object>)provider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }
    }
}