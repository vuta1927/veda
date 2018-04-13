using System.Threading.Tasks;

namespace VDS.Security.Roles
{
    public interface IRoleRemovedEventHandler
    {
        Task RoleRemovedAsync(string roleName);
    }
}