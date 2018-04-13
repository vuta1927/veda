using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.Dependency
{
    public static class ServiceProviderExtensions
    {
        public static List<object> ResolveConstructorDependencies<TService>(
            this IServiceProvider serviceProvider,
            TService decorated,
            IEnumerable<ParameterInfo> constructorParameters)
        {
            var depencenciesList = new List<object>();
            foreach (var parameter in constructorParameters)
            {
                if (parameter.ParameterType == typeof(TService))
                {
                    depencenciesList.Add(decorated);
                }
                else
                {
                    var resolvedDependency = serviceProvider.GetService(parameter.ParameterType);
                    depencenciesList.Add(resolvedDependency);
                }
            }
            return depencenciesList;
        }

        public static List<object> ResolveConstructorDependencies(
            this IServiceProvider serviceProvider,
            IEnumerable<ParameterInfo> constructorParameters)
        {
            return constructorParameters.Select(parameter => serviceProvider.GetService(parameter.ParameterType)).ToList();
        }

        public static bool IsAdded<T>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<T>() != null;
        }
        
        public static void Using<T>(this IServiceProvider serviceProvider, Action<T> action)
        {
            var service = serviceProvider.GetService<T>();
            action(service);
        }
    }
}