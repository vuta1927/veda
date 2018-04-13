using System.Collections.Generic;
using System.Threading.Tasks;

namespace VDS.Security.Roles
{
    public interface IRoleProvider
    {
        Task<List<string>> GetRoleNamesAsync();
        Task<Role> FindByNormalizedRoleNameAsync(string normalizedRoleName);
    }
}