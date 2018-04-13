using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VDS.Caching;
using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;
using VDS.Security;
using VDS.Security.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VDS.AspNetCore.Mvc.Security
{
    public class RoleStore : IRoleClaimStore<Role>, IRoleProvider
    {
        private const string Key = "RolesManager.Roles";
        
        private readonly ISignal _signal;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<RoleClaim, Guid> _roleClaimRepository;
        private readonly IUnitOfWorkManager _uowManager;

        public RoleStore(
            IMemoryCache memoryCache,
            ISignal signal,
            IServiceProvider serviceProvider,
            ILogger<RoleStore> logger, 
            IRepository<Role> roleRepository, 
            IUnitOfWorkManager uowManager,
            IRepository<RoleClaim, Guid> roleClaimRepository)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _serviceProvider = serviceProvider;
            Logger = logger;
            _roleRepository = roleRepository;
            _uowManager = uowManager;
            _roleClaimRepository = roleClaimRepository;
        }

        public ILogger Logger { get; }

        public void Dispose()
        {
        }

        public Task<List<Role>> GetRolesAsync()
        {
            return _memoryCache.GetOrCreateAsync(Key, async entry =>
            {
                var roles = await _roleRepository.GetAllListAsync();
                entry.ExpirationTokens.Add(_signal.GetToken(Key));

                return roles;
            });
        }
        
        public async Task<List<string>> GetRoleNamesAsync()
        {
            var roles = await GetRolesAsync();
            return roles.Select(x => x.RoleName).OrderBy(x => x).ToList();
        }

        public async Task<Role> FindByNormalizedRoleNameAsync(string normalizedRoleName)
        {
            var roles = await GetRolesAsync();
            return roles.FirstOrDefault(x => x.NormalizedRoleName == normalizedRoleName);
        }

        #region IRoleStore<IRole>
        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(role, nameof(role));

            await _uowManager.PerformAsyncUow<Task>(() => _roleRepository.InsertAsync(role));
            _memoryCache.Remove(Key);
            
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(role, nameof(role));
            var appRole = role;

            if (string.Equals(appRole.NormalizedRoleName, "ANONYMOUS") ||
                string.Equals(appRole.NormalizedRoleName, "AUTHENTICATED"))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Can't delete system roles." });
            }

            var roleRemovedEventHandlers = _serviceProvider.GetRequiredService<IEnumerable<IRoleRemovedEventHandler>>();
            await roleRemovedEventHandlers.InvokeAsync(x => x.RoleRemovedAsync(appRole.RoleName), Logger);

            await _uowManager.PerformAsyncUow(() => _roleRepository.DeleteAsync(appRole));

            return IdentityResult.Success;
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var roles = await GetRolesAsync();
            var role = roles.FirstOrDefault(x => x.RoleName == roleId);
            return role;
        }

        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roles = await GetRolesAsync();
            var role = roles.FirstOrDefault(x => x.NormalizedRoleName == normalizedRoleName);
            return role;
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(role, nameof(role));

            return Task.FromResult(role.NormalizedRoleName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(role, nameof(role));

            return Task.FromResult(role.RoleName.ToUpperInvariant());
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(role, nameof(role));

            return Task.FromResult(role.RoleName);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(role, nameof(role));

            role.NormalizedRoleName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(role, nameof(role));

            role.RoleName = roleName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(role, nameof(role));

            await _uowManager.PerformAsyncUow(() => _roleRepository.UpdateAsync(role));

            return IdentityResult.Success;
        }

        #endregion

        #region IRoleClaimStore<IRole>
        public async Task AddClaimAsync(Role role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            Throw.IfArgumentNull(role, nameof(role));
            Throw.IfArgumentNull(claim, nameof(claim));

            await _uowManager.PerformAsyncUow(() => _roleClaimRepository.InsertAsync(new RoleClaim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                RoleId = role.Id
            }));
        }

        public async Task<IList<Claim>> GetClaimsAsync(Role role, CancellationToken cancellationToken = default(CancellationToken))
        {
            Throw.IfArgumentNull(role, nameof(role));

            var claims = await _roleClaimRepository.GetAllListAsync(x => x.RoleId == role.Id);
            return claims.Select(x => x.ToClaim()).ToList();
            
        }

        public async Task RemoveClaimAsync(Role role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            Throw.IfArgumentNull(role, nameof(role));
            Throw.IfArgumentNull(claim, nameof(claim));

            await _uowManager.PerformAsyncUow(() => _roleClaimRepository.DeleteAsync(x =>
                x.ClaimType == claim.Type &&
                x.ClaimValue == claim.Value &&
                x.RoleId == role.Id));
        }

        #endregion
    }
}