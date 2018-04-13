using System.Security.Claims;
using System.Threading.Tasks;
using VDS.Security;
using VDS.Security.Users;
using Microsoft.AspNetCore.Identity;

namespace VDS.AspNetCore.Mvc.Security
{
    public class MembershipService : IMembershipService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserClaimsPrincipalFactory<User> _claimsPrincipalFactory;

        public MembershipService(
            IUserClaimsPrincipalFactory<User> claimsPrincipalFactory,
            UserManager<User> userManager)
        {
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _userManager = userManager;
        }

        public async Task<bool> CheckPasswordAsync(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return false;
            }

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<User> GetUserAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            return user;
        }

        public Task<ClaimsPrincipal> CreateClaimsPrincipal(User user)
        {
            return _claimsPrincipalFactory.CreateAsync(user);
        }
    }
}