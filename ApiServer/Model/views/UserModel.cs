using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class UserModel
    {
        public class User
        {
            public long Id { get; set; }
            public string UserName { get; set; }
            public string NormalizedUserName { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public int? AccessFailedCount { get; set; }
            public bool? IsLockoutEnabled { get; set; }
            public DateTime? LockoutEndDateUtc { get; set; }
            public bool IsActive { get; set; }
            public string NormalizedEmail { get; set; }
            public string PasswordHash { get; set; }
            public string SecurityStamp { get; set; }
            public bool? EmailConfirmed { get; set; }
            public RoleModel.RoleBase Roles { get; set; }
        }

        public class UserForCreateOrEdit
        {
            public int AssignedRoleCount { get; set; }
            public bool isEditMode { get; set; }
            public UserEdit User { get; set; }
            public List<UserRole> Roles { get; set; }
        }

        public class CreateOrUpdateUser
        {
            public bool RandomPassword { get; set; }
            public bool SendActivationEmail { get; set; }
            public UserEdit User { get; set; }
            public List<string> AssignedRoleNames { get; set; }
            public List<string> UnAssignedRoleNames { get; set; }
        }

        public class UserEdit
        {
           public long Id { get; set; }
           public string Name { get; set; }
           public string Surname { get; set; }
           public string Username { get; set; }
            public string EmailAddress { get; set; }
            public string Password { get; set; }
            public bool IsActive { get; set; }
            public bool ShouldChangePasswordOnNextLogin { get; set; }
        }

        public class UserRole
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public string RoleDisplayName { get; set; }
            public bool IsAssigned { get; set; }
        }
    }

}
