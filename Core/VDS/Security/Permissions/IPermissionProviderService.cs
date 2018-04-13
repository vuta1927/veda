using System.Collections.Generic;

namespace VDS.Security.Permissions
{
    public interface IPermissionProviderService
    {
        IEnumerable<Permission> GetPermissions();
        Permission GetPermissionBy(string name);
    }
}