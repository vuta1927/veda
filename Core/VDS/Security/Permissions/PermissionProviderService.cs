using System.Collections.Generic;
using System.Linq;
using VDS.Dependency;
using VDS.Helpers;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace VDS.Security.Permissions
{
    public class PermissionProviderService : IPermissionProviderService, ISingletonDependency
    {
        private readonly IMemoryCache _cache;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;

        public PermissionProviderService(IMemoryCache cache, IEnumerable<IPermissionProvider> permissionProviders)
        {
            _cache = cache;
            _permissionProviders = permissionProviders;
            RegisterPermission();
        }

        private void RegisterPermission()
        {
            var permissions = new HashSet<Permission>();
            foreach (var permission in _permissionProviders.SelectMany(x => x.GetPermissions()))
            {
                GetPermissions(permission, permissions);
                permissions.ForEach(p => Permissions.Add(new PermissionInfo(p)));
            }
        }

        private void GetPermissions(Permission permission, HashSet<Permission> stack)
        {
            // The given name is tested
            stack.Add(permission);

            // Iterate implied permissions to grant, it present
            if (permission.Children != null && permission.Children.Any())
            {
                foreach (var child in permission.Children)
                {
                    // Avoid potential recursion
                    if (stack.Any(x => x.Name.Contains(child.Name)))
                    {
                        continue;
                    }

                    // Otherwise accumulate the implied permission names recursively
                    GetPermissions(child, stack);
                }
            }
        }

        protected ISet<PermissionInfo> Permissions
        {
            get
            {
                return _cache.GetOrCreate("Permissions",
                    entry => new HashSet<PermissionInfo>(new KeyEqualityComparer<PermissionInfo>(pi => pi.Permission)));
            }
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return Permissions.Select(x => x.Permission);
        }

        public Permission GetPermissionBy(string name)
        {
            var permissionInfo = Permissions.SingleOrDefault(pi => pi.Permission.Name.EqualsIgnoreCase(name));
            if (permissionInfo == null)
                return null;
            return permissionInfo.Permission;
        }

        protected class PermissionInfo
        {
            public PermissionInfo(Permission permission)
            {
                Throw.IfArgumentNull(permission, nameof(permission));
                Permission = permission;
            }

            public Permission Permission { get; private set; }
        }
    }
}