using System.Threading.Tasks;
using VDS.Security;
using VDS.Security.Roles;
using Microsoft.AspNetCore.Identity;

namespace VDS.AspNetCore.Mvc.Security
{
    public class UserRoleRemovedEventHandler : IRoleRemovedEventHandler
    {
        private readonly UserManager<User> _userManager;

        public UserRoleRemovedEventHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task RoleRemovedAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);

            foreach (var user in users)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }
        }
    }
}