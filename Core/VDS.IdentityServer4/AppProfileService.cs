using System.Threading.Tasks;
using VDS.Security;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;

namespace VDS.IdentityServer4
{
    public class AppProfileService : ProfileService<User>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public AppProfileService(
            UserManager<User> userManager,
            IUserClaimsPrincipalFactory<User> claimsFactory,
            RoleManager<Role> roleManager)
            : base(userManager, claimsFactory)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);

            var user = await _userManager.GetUserAsync(context.Subject);

            var roleNames = await _userManager.GetRolesAsync(user);
            if (roleNames.IsNullOrEmpty()) return;

            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                context.IssuedClaims.AddRange(roleClaims);
            }
        }
    }
}