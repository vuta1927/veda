using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VDS.Security.Users
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(string userName, string email, string[] roleNames, string password, Action<string, string> reportError);
        Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword, Action<string, string> reportError);
        Task<User> GetAuthenticatedUserAsync(ClaimsPrincipal principal);
    }
}