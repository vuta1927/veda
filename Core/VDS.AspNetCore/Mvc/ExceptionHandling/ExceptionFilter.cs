using System.Net;
using VDS.AspNetCore.Mvc.Extensions;
using VDS.AspNetCore.Mvc.Models;
using VDS.AspNetCore.Mvc.Results;
using VDS.Dependency;
using VDS.Entities;
using VDS.Security;
using VDS.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace VDS.AspNetCore.Mvc.ExceptionHandling
{
    public class ExceptionFilter : IExceptionFilter, ITransientDependency
    {
        private readonly IErrorInfoBuilder _errorInfoBuilder;
        private readonly ILogger _logger;

        public ExceptionFilter(IErrorInfoBuilder errorInfoBuilder, ILogger<ExceptionFilter> logger)
        {
            _errorInfoBuilder = errorInfoBuilder;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (!context.ActionDescriptor.IsControllerAction())
                return;

            _logger.LogError(context.Exception, null, null);

            HandleAndWrapException(context);
        }

        private void HandleAndWrapException(ExceptionContext context)
        {
            if (!ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
                return;

            context.HttpContext.Response.StatusCode = GetStatusCode(context);

            context.Result = new ObjectResult(
                    new AjaxResponse(
                            _errorInfoBuilder.BuildForException(context.Exception),
                            context.Exception is SecurityException
                        )
                );

            context.Exception = null; // Handled!
        }

        private int GetStatusCode(ExceptionContext context)
        {
            if (context.Exception is SecurityException)
            {
                return context.HttpContext.User.Identity.IsAuthenticated
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
            }

            if (context.Exception is ValidationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            if (context.Exception is EntityNotFoundException)
            {
                return (int)HttpStatusCode.NotFound;
            }

            return (int)HttpStatusCode.InternalServerError;
        }
    }
}