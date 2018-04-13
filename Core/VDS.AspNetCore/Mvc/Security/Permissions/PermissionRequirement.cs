using VDS.Helpers.Exception;
using VDS.Security.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace VDS.AspNetCore.Mvc.Security.Permissions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(Permission permission)
        {
            Throw.IfArgumentNull(permission, nameof(permission));
            Permission = permission;
        }

        public Permission Permission { get; set; }
    }
}