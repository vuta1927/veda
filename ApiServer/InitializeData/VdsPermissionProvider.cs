using System.Collections.Generic;
using VDS.Security.Permissions;
namespace ApiServer.InitializeData
{
    class VdsPermissionProvider: IPermissionProvider
    {
        public static readonly Permission AdministratorPermissions = new Permission { Name = VdsPermissions.Administrator, DisplayName = "Administrator", Description = "Administrator permission", Category = "ADMIN" };

        public static readonly Permission ViewUserPermission = new Permission { Name = VdsPermissions.ViewUser, DisplayName = "View", Description = "Allow view users", Category = "USER" };
        public static readonly Permission EditUserPermission = new Permission { Name = VdsPermissions.EditUser, DisplayName = "Edit", Description = "Allow to edit user profile", Category = "USER" };
        public static readonly Permission AddUserPermission = new Permission { Name = VdsPermissions.AddUser, DisplayName = "Add", Description = "Allow to add create user", Category = "USER" };
        public static readonly Permission DeleteUserPermission = new Permission { Name = VdsPermissions.DeleteUser, DisplayName = "Delete", Description = "Allow to delete user", Category = "USER" };

        public static readonly Permission ViewRolePermission = new Permission { Name = VdsPermissions.ViewRole, DisplayName = "View", Description = "Allow to view roles", Category = "ROLE" };
        public static readonly Permission EditRolePermission = new Permission { Name = VdsPermissions.EditRole, DisplayName = "Edit", Description = "Allow to edit role", Category = "ROLE" };
        public static readonly Permission AddRolePermission = new Permission { Name = VdsPermissions.AddRole, DisplayName = "Add", Description = "Allow to add role", Category = "ROLE" };
        public static readonly Permission DeleteRolePermission = new Permission { Name = VdsPermissions.DeleteRole, DisplayName = "Delete", Description = "Allow to delete role", Category = "ROLE" };

        public static readonly Permission ViewProject = new Permission { Name = VdsPermissions.ViewProject, DisplayName = "View", Description = "Allow to view project", Category = VdsPermissions.ProjectCategory };
        public static readonly Permission AddProject = new Permission { Name = VdsPermissions.AddProject, DisplayName = "Add", Description = "Allow to add project", Category = VdsPermissions.ProjectCategory };
        public static readonly Permission EditProject = new Permission { Name = VdsPermissions.EditProject, DisplayName = "Edit", Description = "Allow to edit project", Category = VdsPermissions.ProjectCategory };
        public static readonly Permission DeleteProject = new Permission { Name = VdsPermissions.DeleteProject, DisplayName = "Delete", Description = "Allow to delete project", Category = VdsPermissions.ProjectCategory };

        public static readonly Permission ViewImage = new Permission { Name = VdsPermissions.ViewImage, DisplayName = "View", Description = "Allow to view image", Category = VdsPermissions.ImageCategory };
        public static readonly Permission AddImage = new Permission { Name = VdsPermissions.AddImage, DisplayName = "Add", Description = "Allow to add image", Category = VdsPermissions.ImageCategory };
        public static readonly Permission EditImage = new Permission { Name = VdsPermissions.EditImage, DisplayName = "Edit", Description = "Allow to modify info image", Category = VdsPermissions.ImageCategory };
        public static readonly Permission DeleteImage = new Permission { Name = VdsPermissions.DeleteImage, DisplayName = "Delete", Description = "Allow to delete image", Category = VdsPermissions.ImageCategory };

        public static readonly Permission ViewQc = new Permission { Name = VdsPermissions.ViewQc, DisplayName = "View", Description = "Allow to view Qc", Category = VdsPermissions.QcCategory };
        public static readonly Permission AddQc = new Permission { Name = VdsPermissions.AddQc, DisplayName = "Add", Description = "Allow to add Qc", Category = VdsPermissions.QcCategory };
        public static readonly Permission EditQc = new Permission { Name = VdsPermissions.EditQc, DisplayName = "Edit", Description = "Allow to modify Qc", Category = VdsPermissions.QcCategory };
        public static readonly Permission DeleteQc = new Permission { Name = VdsPermissions.DeleteQc, DisplayName = "Delete", Description = "Allow to delete Qc", Category = VdsPermissions.QcCategory };

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                AdministratorPermissions,

                AddUserPermission,
                EditUserPermission,
                DeleteUserPermission,
                ViewUserPermission,

                AddRolePermission,
                EditRolePermission,
                DeleteRolePermission,
                ViewRolePermission,

                ViewProject,
                AddProject,
                EditProject,
                DeleteProject,

                ViewImage,
                AddImage,
                EditImage,
                DeleteImage,

                ViewQc,
                AddQc,
                EditQc,
                DeleteQc
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Admin group",
                    Permissions = new[] { AdministratorPermissions }
                },
                //new PermissionStereotype
                //{
                //    Name = "Administrator",
                //    Permissions = new []{Page, AdminPermission, ViewMapPermission, EditMapPermission }
                //}
            };
        }
    }
}
