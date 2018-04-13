using System;
using System.Collections.Generic;

namespace VDS.Dependency.Registration
{
    /// <summary>
    ///   Selects an existing set of types to register.
    /// </summary>
    public class FromTypesDescriptor : FromDescriptor
    {
        private readonly IEnumerable<Type> _types;

        internal FromTypesDescriptor(IEnumerable<Type> types, Predicate<Type> additionalFilters) : base(additionalFilters)
        {
            _types = types;
        }

        protected override IEnumerable<Type> SelectedTypes()
        {
            return _types;
        }
    }
}