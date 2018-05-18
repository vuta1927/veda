using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VDS.Security;

namespace ApiServer.Core.Authorization
{
    public interface IUserService
    {
        User GetCurrentUser(ClaimsIdentity identity);

        ICollection<Role> GetCurrentRole(long UserId);
    }
}
