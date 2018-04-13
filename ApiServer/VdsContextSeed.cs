using ApiServer.InitializeData;
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

namespace ApiServer
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
                await AddQcType(_ctx);

                await AddUser();
            });
        }

        private async Task AddUser()
        {
            using (_ctx)
            {
                _ctx.Database.Migrate();
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

                        _ctx.UserRoles.Add(new UserRole(adminUser.Id, adminRole.Id));

                        _ctx.SaveChanges();


                    }
                }

                await AddPermision(_ctx);
            }
        }

        private async Task AddPermision(VdsContext _ctx)
        {
            var adminRole = await _ctx.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");
            if (adminRole != null)
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

                        _ctx.PermissionRoles.Add(new Models.PermissionRole { PermissionId = newPermission.Id, RoleId = adminRole.Id });

                        _ctx.SaveChanges();
                    }
                }

            }
        }

        private async Task AddQcType(VdsContext _ctx)
        {
            if (!_ctx.QuantityCheckTypes.Any())
            {
                var qcProvider = new QcProvider();

                var QcTemplates = qcProvider.GetQcs();

                await _ctx.AddRangeAsync(QcTemplates);

                _ctx.SaveChanges();
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
