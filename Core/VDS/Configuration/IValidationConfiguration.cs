using System;
using System.Collections.Generic;

namespace VDS.Configuration
{
    public interface IValidationConfiguration : IConfigurator
    {
        List<Type> IgnoredTypes { get; }
    }
}