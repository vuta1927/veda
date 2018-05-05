using ApiServer.Core.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.Security;
using VDS.Storage.EntityFrameworkCore;

namespace ApiServer.Model
{
    public class VdsContextSeed : IDbContextSeed
    {
        private readonly ILogger<VdsContext> _logger;
        private readonly VdsContext _ctx;
        public VdsContextSeed(ILogger<VdsContext> logger, VdsContext context)
        {
            _logger = logger;
            _ctx = context;
        }
        public Type ContextType => typeof(VdsContext);
        public async Task SeedAsync()
        {
            var policy = CreatePolicy(nameof(VdsContext));

            await policy.ExecuteAsync(async () =>
            {
                using (_ctx)
                {
                    await AddUser(_ctx);
                }
            });
        }

        private async Task AddUser(VdsContext _ctx)
        {
            _ctx.Database.Migrate();

            var projectManagerRole = await _ctx.Roles.FirstOrDefaultAsync(r => r.RoleName == "ProjectManager");
            if (projectManagerRole == null)
            {
                projectManagerRole = new Role
                {
                    RoleName = "ProjectManager",
                    NormalizedRoleName = "PROJECTMANAGER",
                    ProjectRole = true
                };
                _ctx.Roles.Add(projectManagerRole);
                await _ctx.SaveChangesAsync();
            }

            var TeacherRole = await _ctx.Roles.FirstOrDefaultAsync(r => r.RoleName == "Teacher");
            if (TeacherRole == null)
            {
                TeacherRole = new Role
                {
                    RoleName = "Teacher",
                    NormalizedRoleName = "TEACHER",
                    ProjectRole = true
                };
                _ctx.Roles.Add(TeacherRole);
                await _ctx.SaveChangesAsync();
            }

            var QcRole = await _ctx.Roles.FirstOrDefaultAsync(r => r.RoleName == "QuantityCheck");
            if (QcRole == null)
            {
                QcRole = new Role
                {
                    RoleName = "QuantityCheck",
                    NormalizedRoleName = "QUANTITYCHECK",
                    ProjectRole = true
                };
                _ctx.Roles.Add(QcRole);
                await _ctx.SaveChangesAsync();
            }


            if (!_ctx.Users.Any(x => x.Email == "admin@demo.com"))
            {
                // Add 'administrator' role
                var adminRole = await _ctx.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");
                if (adminRole == null)
                {
                    adminRole = new Role
                    {
                        RoleName = "Administrator",
                        NormalizedRoleName = "ADMINISTRATOR"
                    };
                    _ctx.Roles.Add(adminRole);
                    await _ctx.SaveChangesAsync();
                }

                // Create admin user
                var adminUser = _ctx.Users.FirstOrDefault(u => u.UserName == "admin");
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = "admin",
                        NormalizedUserName = "ADMIN",
                        Name = "admin",
                        Surname = "admin",
                        Email = "admin@demo.com",
                        NormalizedEmail = "ADMIN@DEMO.COM",
                        IsActive = true,
                        EmailConfirmed = true,
                        PasswordHash = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    };

                    _ctx.Users.Add(adminUser);

                    _ctx.SaveChanges();

                    _ctx.UserRoles.AddRange(
                        new UserRole(adminUser.Id, adminRole.Id),
                        new UserRole(adminUser.Id, projectManagerRole.Id)
                        );

                    _ctx.SaveChanges();
                }
            }

            if (!_ctx.Users.Any(x => x.Email == "teacher@demo.com"))
            {
                var pmUser = _ctx.Users.FirstOrDefault(u => u.UserName == "teacher");
                if (pmUser == null)
                {
                    pmUser = new User
                    {
                        UserName = "pm",
                        NormalizedUserName = "PM",
                        Name = "pm",
                        Surname = "pm",
                        Email = "pm@demo.com",
                        NormalizedEmail = "PM@DEMO.COM",
                        IsActive = true,
                        EmailConfirmed = true,
                        PasswordHash = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    };

                    _ctx.Users.Add(pmUser);

                    _ctx.SaveChanges();

                    _ctx.UserRoles.Add(new UserRole(pmUser.Id, projectManagerRole.Id));

                    _ctx.SaveChanges();
                }
            }

            if (!_ctx.Users.Any(x => x.Email == "teacher@demo.com"))
            {
                var testUser = _ctx.Users.FirstOrDefault(u => u.UserName == "teacher");
                if (testUser == null)
                {
                    testUser = new User
                    {
                        UserName = "teacher",
                        NormalizedUserName = "TEACHER",
                        Name = "teacher",
                        Surname = "teacher",
                        Email = "teacher@demo.com",
                        NormalizedEmail = "TEACHER@DEMO.COM",
                        IsActive = true,
                        EmailConfirmed = true,
                        PasswordHash = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    };

                    _ctx.Users.Add(testUser);

                    _ctx.SaveChanges();

                    _ctx.UserRoles.Add(new UserRole(testUser.Id, TeacherRole.Id));

                    _ctx.SaveChanges();
                }
            }

            if (!_ctx.Users.Any(x => x.Email == "qc@demo.com"))
            {
                var qcUser = _ctx.Users.FirstOrDefault(u => u.UserName == "qc");
                if (qcUser == null)
                {
                    qcUser = new User
                    {
                        UserName = "qc",
                        NormalizedUserName = "qc",
                        Name = "qc",
                        Surname = "qc",
                        Email = "qc@demo.com",
                        NormalizedEmail = "QC@DEMO.COM",
                        IsActive = true,
                        EmailConfirmed = true,
                        PasswordHash = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    };

                    _ctx.Users.Add(qcUser);

                    _ctx.SaveChanges();

                    _ctx.UserRoles.Add(new UserRole(qcUser.Id, QcRole.Id));

                    _ctx.SaveChanges();
                }
            }

            await AddPermision(_ctx);

        }

        private async Task AddPermision(VdsContext _ctx)
        {
            var mangementPermissions = new VdsPermissionProvider();
            var permissions = mangementPermissions.GetPermissions();
            foreach (var permission in permissions)
            {
                if (!_ctx.Permissions.Any(x => x.Name == permission.Name))
                {
                    var newPermission = new VDS.Security.Permissions.Permission
                    {
                        Name = permission.Name,
                        Category = permission.Category,
                        Description = permission.Description,
                        DisplayName = permission.DisplayName
                    };

                    _ctx.Permissions.Add(newPermission);

                    _ctx.SaveChanges();
                }
            }

            var adminRole = await _ctx.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");
            if (adminRole != null)
            {
                foreach (var permission in permissions)
                {
                    if (permission.Category == VdsPermissions.ProjectCategory ||
                        permission.Category == VdsPermissions.ImageCategory ||
                        permission.Category == VdsPermissions.QcCategory ||
                        permission.Category == VdsPermissions.TagCategory ||
                        permission.Category == VdsPermissions.UserCategory ||
                        permission.Category == VdsPermissions.RoleCategory ||
                        permission.Name == VdsPermissions.Administrator)
                    {
                        var permissionInDatabase = await _ctx.Permissions.SingleOrDefaultAsync(x => x.Name == permission.Name);
                        if (permissionInDatabase != null)
                        {
                            _ctx.PermissionRoles.Add(new PermissionRole { PermissionId = permissionInDatabase.Id, RoleId = adminRole.Id });
                        }
                    }
                }
            }

            var projectRole = await _ctx.Roles.FirstOrDefaultAsync(r => r.RoleName == "ProjectManager");
            if (projectRole != null)
            {
                foreach (var permission in permissions)
                {
                    if (permission.Category == VdsPermissions.ProjectCategory ||
                        permission.Category == VdsPermissions.ImageCategory ||
                        permission.Category == VdsPermissions.QcCategory ||
                        permission.Category == VdsPermissions.TagCategory ||
                        permission.Category == VdsPermissions.UserCategory ||
                        permission.Category == VdsPermissions.RoleCategory)
                    {
                        var permissionInDatabase = await _ctx.Permissions.SingleOrDefaultAsync(x => x.Name == permission.Name);
                        if (permissionInDatabase != null)
                        {
                            _ctx.PermissionRoles.Add(new PermissionRole { PermissionId = permissionInDatabase.Id, RoleId = projectRole.Id });
                        }
                    }
                }
            }

            var teacherRole = await _ctx.Roles.FirstOrDefaultAsync(x => x.RoleName == "Teacher");
            if(teacherRole != null)
            {
                foreach (var permission in permissions)
                {
                    if (permission.Name == VdsPermissions.ViewProject || 
                        permission.Name == VdsPermissions.ViewQc || 
                        permission.Category == VdsPermissions.ImageCategory ||  
                        permission.Category == VdsPermissions.TagCategory)
                    {
                        var permissionInDatabase = await _ctx.Permissions.SingleOrDefaultAsync(x => x.Name == permission.Name);
                        if (permissionInDatabase != null)
                        {
                            _ctx.PermissionRoles.Add(new PermissionRole { PermissionId = permissionInDatabase.Id, RoleId = teacherRole.Id });
                        }
                    }
                }
            }

            var qcRole = await _ctx.Roles.FirstOrDefaultAsync(x => x.RoleName == "QuantityCheck");
            if (qcRole != null)
            {
                foreach (var permission in permissions)
                {
                    if (permission.Name == VdsPermissions.ViewProject || 
                        permission.Category == VdsPermissions.QcCategory || 
                        permission.Name == VdsPermissions.ViewImage || 
                        permission.Name == VdsPermissions.ViewTag)
                    {
                        var permissionInDatabase = await _ctx.Permissions.SingleOrDefaultAsync(x => x.Name == permission.Name);
                        if (permissionInDatabase != null)
                        {
                            _ctx.PermissionRoles.Add(new PermissionRole { PermissionId = permissionInDatabase.Id, RoleId = qcRole.Id });
                        }
                    }
                }
            }

            try {
                await _ctx.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        private Policy CreatePolicy(string prefix, int retries = 3)
        {
            return Policy.Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        _logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
                    }
                );
        }
    }
}
