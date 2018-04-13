using VDS.AspNetCore.Mvc.Authorization;
using VDS.AspNetCore.Mvc.ExceptionHandling;
using VDS.AspNetCore.Mvc.Results;
using VDS.AspNetCore.Mvc.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.AspNetCore.Mvc
{
    internal static class MvcOptionsExtensions
    {
        public static void AddDomain(this MvcOptions options, IServiceCollection services)
        {
            AddFilter(options);
        }

        private static void AddFilter(MvcOptions options)
        {
            options.Filters.Add(typeof(AuthorizationFilter));
            options.Filters.Add(typeof(ValidationActionFilter));
//            options.Filters.Add(typeof(UowActionFilter));
            options.Filters.Add(typeof(ExceptionFilter));
            options.Filters.Add(typeof(AppResultFilter));
        }
    }
}