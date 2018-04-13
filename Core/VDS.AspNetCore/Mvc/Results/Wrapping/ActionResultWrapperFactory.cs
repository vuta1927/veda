using VDS.Helpers.Exception;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VDS.AspNetCore.Mvc.Results.Wrapping
{
    public class ActionResultWrapperFactory : IActionResultWrapperFactory
    {
        public IActionResultWrapper CreateFor(ResultExecutingContext actionResult)
        {
            Throw.IfArgumentNull(actionResult, nameof(actionResult));

            if (actionResult.Result is ObjectResult)
            {
                return new ObjectActionResultWrapper(actionResult.HttpContext.RequestServices);
            }

            if (actionResult.Result is JsonResult)
            {
                return new JsonActionResultWrapper();
            }

            if (actionResult.Result is EmptyResult)
            {
                return new EmptyActionResultWrapper();
            }

            return new NullActionResultWrapper();
        }
    }
}