using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VDS.Configuration;
using VDS.Data.Uow;
using VDS.Dependency;
using VDS.Security;
using VDS.Security.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace VDS.AspNetCore.Mvc.Security
{
    public class RoleUpdater : IWantToKnowWhenConfigurationIsDone, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ILogger _logger;

        public RoleUpdater(IUnitOfWorkManager unitOfWorkManager,
            RoleManager<Role> roleManager,
            IEnumerable<IPermissionProvider> permissionProviders,
            ILogger<RoleUpdater> logger)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _roleManager = roleManager;
            _permissionProviders = permissionProviders;
            _logger = logger;
        }

        public async Task Configured(IConfigure configure)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                foreach (var permissionProvider in _permissionProviders)
                {
                    // get and iterate stereotypical groups of permissions
                    var stereotypes = permissionProvider.GetDefaultStereotypes();
                    foreach (var stereotype in stereotypes)
                    {
                        var role = await _roleManager.FindByNameAsync(stereotype.Name);
                        if (role == null)
                        {
                            if (_logger.IsEnabled(LogLevel.Information))
                            {
                                _logger.LogInformation(
                                    $"Defining new role {stereotype.Name} for permission stereotype");
                            }

                            role = new Role { RoleName = stereotype.Name };
                            await _roleManager.CreateAsync(role);
                            await _unitOfWorkManager.Current.SaveChangesAsync();
                        }

                        // and merge the stereotypical permissions into that role
                        var stereotypePermissionNames =
                            (stereotype.Permissions ?? Enumerable.Empty<Permission>()).Select(x => x.Name);
                        var currentPermissionNames = (await _roleManager.GetClaimsAsync(role)).Where(x => x.Type == Permission.ClaimType)
                            .Select(x => x.Value).ToList();

                        var distinctPermissionNames = currentPermissionNames
                            .Union(stereotypePermissionNames)
                            .Distinct();

                        // update role if set of permissions has increased
                        var additionalPermissionNames = distinctPermissionNames.Except(currentPermissionNames).ToList();

                        if (additionalPermissionNames.Any())
                        {
                            foreach (var permissionName in additionalPermissionNames)
                            {
                                if (_logger.IsEnabled(LogLevel.Debug))
                                {
                                    _logger.LogInformation("Default role {0} granted permission {1}", stereotype.Name,
                                        permissionName);
                                }

                                await _roleManager.AddClaimAsync(role, new Claim(Permission.ClaimType, permissionName));
                            }
                        }
                    }
                }
            });
        }
    }
}