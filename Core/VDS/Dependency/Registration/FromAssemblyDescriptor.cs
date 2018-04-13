using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.Helpers.Reflection;

namespace VDS.Dependency.Registration
{
    /// <summary>
    ///   Selects a set of types from an assembly.
    /// </summary>
    public class FromAssemblyDescriptor : FromDescriptor
    {
        protected readonly IEnumerable<Assembly> Assemblies;
        protected bool NonPublicTypes;

        protected internal FromAssemblyDescriptor(Assembly assembly, Predicate<Type> additionalFilters) : base(additionalFilters)
        {
            Assemblies = new[] { assembly };
        }

        protected internal FromAssemblyDescriptor(IEnumerable<Assembly> assemblies, Predicate<Type> additionalFilters)
            : base(additionalFilters)
        {
            Assemblies = assemblies;
        }

        /// <summary>
        ///   When called also non-public types will be scanned.
        /// </summary>
        /// <remarks>
        ///   Usually it is not recommended to register non-public types in the container so think twice before using this option.
        /// </remarks>
        public FromAssemblyDescriptor IncludeNonPublicTypes()
        {
            NonPublicTypes = true;
            return this;
        }

        protected override IEnumerable<Type> SelectedTypes()
        {
            return Assemblies.SelectMany(a => a.GetAvailableTypesOrdered(NonPublicTypes));
        }
    }
}