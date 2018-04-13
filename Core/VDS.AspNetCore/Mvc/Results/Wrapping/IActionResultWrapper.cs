using Microsoft.AspNetCore.Mvc.Filters;

namespace VDS.AspNetCore.Mvc.Results.Wrapping
{
    public interface IActionResultWrapper
    {
        void Wrap(ResultExecutingContext actionResult);
    }
}