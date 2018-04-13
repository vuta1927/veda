using VDS.AspNetCore.Mvc.Extensions;
using VDS.AspNetCore.Mvc.Results.Wrapping;
using VDS.Dependency;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VDS.AspNetCore.Mvc.Results
{
    public class AppResultFilter : IResultFilter, ITransientDependency
    {
        private readonly IActionResultWrapperFactory _actionResultWrapperFactory;

        public AppResultFilter(IActionResultWrapperFactory actionResultWrapper)
        {
            _actionResultWrapperFactory = actionResultWrapper;
        }

        public virtual void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }
            
            _actionResultWrapperFactory.CreateFor(context).Wrap(context);
        }

        public virtual void OnResultExecuted(ResultExecutedContext context)
        {
            //no action
        }
    }
}