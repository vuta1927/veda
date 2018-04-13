using VDS.Dependency;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VDS.AspNetCore.Mvc.Results.Wrapping
{
    public interface IActionResultWrapperFactory : ITransientDependency
    {
        IActionResultWrapper CreateFor([NotNull] ResultExecutingContext actionResult);
    }
}