using System.Security.Claims;
using System.Security.Principal;
using VDS.AspNetCore.Security.AntiForgery;
using Microsoft.AspNetCore.Http;

namespace VDS.AspNetCore.Web.Security.AntiForgery
{
    public static class AntiForgeryManagerAspNetCoreExtensions
    {
        public static void SetCookie(this IAntiForgeryManager manager, HttpContext context, IIdentity identity = null)
        {
            if (identity != null)
            {
                context.User = new ClaimsPrincipal(identity);
            }

            context.Response.Cookies.Append(manager.Configuration.TokenCookieName, manager.GenerateToken());
        }
    }
}