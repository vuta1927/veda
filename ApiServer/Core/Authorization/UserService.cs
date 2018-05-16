using ApiServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VDS.Security;

namespace ApiServer.Core.Authorization
{
    public class UserService : IUserService
    {
        private readonly VdsContext _context;
        public UserService(VdsContext vdsContext)
        {
            _context = vdsContext;
        }

        public User GetCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            foreach (var claim in claims)
            {
                if (claim.Type == "id" && !string.IsNullOrEmpty(claim.Value))
                {
                    var userId = long.Parse(claim.Value);
                    return _context.Users.SingleOrDefault(x => x.Id == userId);
                }
            }

            return null;
        }

        public ICollection<Role> GetCurrentRole(long userId)
        {
            var userRoles = _context.UserRoles.Where(x => x.UserId == userId);
            var result = new List<Role>();
            foreach (var r in userRoles)
            {
                result.Add(_context.Roles.SingleOrDefault(x => x.Id == r.RoleId));
            }
            return result;
        }
    }
}
