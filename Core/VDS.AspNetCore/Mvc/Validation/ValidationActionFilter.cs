using System;
using System.Threading.Tasks;
using VDS.AspNetCore.Mvc.Extensions;
using VDS.Dependency;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.AspNetCore.Mvc.Validation
{
    public class ValidationActionFilter : IAsyncActionFilter, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationActionFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                await next();
                return;
            }

            var validator = _serviceProvider.GetService<MvcActionInvocationValidator>();

            validator.Initialize(context);
            validator.Validate();

            await next();
        }
    }
}