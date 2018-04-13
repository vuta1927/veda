using System.Security.Claims;
using System.Threading.Tasks;
using VDS.AspNetCore.Mvc.Security.Permissions;
using VDS.Security.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace VDS.AspNetCore.Mvc.Security
{
    public static class AuthorizationServiceExtensions
    {
        public static Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user,
            Permission permission)
        {
            return AuthorizeAsync(service, user, permission, null);
        }

        public static async Task<bool> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user,
            Permission permission, object resource)
        {
            return (await service.AuthorizeAsync(user, resource, new PermissionRequirement(permission))).Succeeded;
        }
    }
}