using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Dependency;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;
using VDS.Security;
using VDS.Security.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace VDS.AspNetCore.Mvc.Security
{
    public class UserStore :
        IUserLoginStore<User>,
        IUserRoleStore<User>,
        IUserPasswordStore<User>,
        IUserEmailStore<User>,
        IUserSecurityStampStore<User>,

        ITransientDependency
    {

        /// <summary>
        /// Gets or sets a flag indicating if changes should be persisted after CreateAsync, UpdateAsync and DeleteAsync are called.
        /// </summary>
        /// <value>
        /// True if changes should be automatically persisted, otherwise false.
        /// </value>
        public bool AutoSaveChanges { get; set; } = true;

        private readonly IRoleProvider _roleProvider;
        private readonly ILookupNormalizer _keyNormalizer;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserLogin, long> _userLoginRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger _logger;

        public UserStore(
            IRoleProvider roleProvider,
            ILookupNormalizer keyNormalizer,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<User, long> userRepository,
            IRepository<UserLogin, long> userLoginRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole, long> userRoleRepository,
            ILogger<UserStore> logger)
        {
            _roleProvider = roleProvider;
            _keyNormalizer = keyNormalizer;
            _unitOfWorkManager = unitOfWorkManager;
            _userRepository = userRepository;
            _userLoginRepository = userLoginRepository;
            _logger = logger;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        /// <summary>Saves the current store.</summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected Task SaveChanges(CancellationToken cancellationToken)
        {
            if (!AutoSaveChanges || _unitOfWorkManager.Current == null)
            {
                return Task.CompletedTask;
            }

            return _unitOfWorkManager.Current.SaveChangesAsync();
        }

        public void Dispose()
        {
        }

        public string NormalizeKey(string key)
        {
            return _keyNormalizer == null ? key : _keyNormalizer.Normalize(key);
        }

        #region IUserLoginStore

        public virtual async Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _userLoginRepository.InsertAsync(
                new UserLogin
                {
                    LoginProvider = login.LoginProvider,
                    ProviderKey = login.ProviderKey,
                    UserId = user.Id,
                    ProviderDisplayName = login.ProviderDisplayName
                });
        }

        public async Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _userLoginRepository.DeleteAsync(
                ul => ul.UserId == user.Id &&
                      ul.LoginProvider == loginProvider &&
                      ul.ProviderKey == providerKey
            );
        }

        public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return (await _userLoginRepository.GetAllListAsync(ul => ul.UserId == user.Id))
                .Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName))
                .ToList();
        }

        public async Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userLogin = await _userLoginRepository.FirstOrDefaultAsync(
                ul => ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey
            );

            if (userLogin == null)
            {
                return null;
            }

            return await _userRepository.FirstOrDefaultAsync(u => u.Id == userLogin.UserId);
        }
        
        #endregion

        #region IUserStore<IUser>
        public virtual async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            await _userRepository.InsertAsync(user);
            await SaveChanges(cancellationToken);

            return IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            await _userRepository.DeleteAsync(user);

            try
            {
                await SaveChanges(cancellationToken);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex.ToString(), ex);
                return IdentityResult.Failed();
            }

            await SaveChanges(cancellationToken);

            return IdentityResult.Success;
        }

        public virtual Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _userRepository.GetAsync(userId.To<long>());
        }

        public virtual Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(normalizedUserName, nameof(normalizedUserName));

            return _userRepository.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);
        }

        public virtual Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.NormalizedUserName);
        }

        public virtual Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.Id.ToString());
        }

        public virtual Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.UserName);
        }

        public virtual Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            user.NormalizedUserName = normalizedName;

            return Task.CompletedTask;
        }

        public virtual Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            user.UserName = userName;

            return Task.CompletedTask;
        }

        public virtual async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            await _userRepository.UpdateAsync(user);

            try
            {
                await SaveChanges(cancellationToken);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex.ToString(), ex);
                return IdentityResult.Failed();
            }

            await SaveChanges(cancellationToken);

            return IdentityResult.Success;
        }

        #endregion

        #region IUserPasswordStore<IUser>
        public virtual Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.PasswordHash);
        }

        public virtual Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public virtual Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.PasswordHash != null);
        }

        #endregion

        #region ISecurityStampValidator<IUser>
        public virtual Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            Throw.IfArgumentNull(user, nameof(user));

            user.SecurityStamp = stamp;

            return Task.CompletedTask;
        }

        public virtual Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            Throw.IfArgumentNull(user, nameof(user));
            return Task.FromResult(user.SecurityStamp);
        }
        #endregion

        #region IUserEmailStore<IUser>
        public virtual Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(user, nameof(user));

            user.Email = email;

            return Task.CompletedTask;
        }

        public virtual Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.Email);
        }

        public virtual Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.EmailConfirmed);
        }

        public virtual Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(user, nameof(user));

            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public virtual Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return _userRepository.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        }

        public virtual Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(user, nameof(user));

            return Task.FromResult(user.NormalizedEmail);
        }

        public virtual Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            Throw.IfArgumentNull(user, nameof(user));

            user.NormalizedEmail = normalizedEmail;

            return Task.CompletedTask;
        }

        #endregion

        #region IUserRoleStore<IUser>
        public virtual async Task AddToRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));
            Throw.IfArgumentNull(normalizedRoleName, nameof(normalizedRoleName));

            if (await IsInRoleAsync(user, normalizedRoleName, cancellationToken))
            {
                return;
            }

            var role = await _roleProvider.FindByNormalizedRoleNameAsync(normalizedRoleName);

            if (role == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist!", normalizedRoleName));
            }

            user.Roles.Add(new UserRole(user.Id, role.Id));
        }

        public virtual async Task RemoveFromRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException(nameof(normalizedRoleName) + " can not be null or whitespace");
            }

            if (!await IsInRoleAsync(user, normalizedRoleName, cancellationToken))
            {
                return;
            }

            var role = await _roleProvider.FindByNormalizedRoleNameAsync(normalizedRoleName);
            if (role == null)
            {
                return;
            }

            user.Roles.RemoveAll(r => r.RoleId == role.Id);
        }

        public virtual async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            return await _unitOfWorkManager.PerformAsyncUow<Task<IList<string>>>(async () =>
            {
                var query = from userRole in _userRoleRepository.GetAll()
                    join role in _roleRepository.GetAll() on userRole.RoleId equals role.Id
                    where userRole.UserId == user.Id
                    select role.RoleName;

                return await Task.FromResult(query.ToList());
            });
        }

        public virtual async Task<bool> IsInRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(user, nameof(user));

            var role = await _roleProvider.FindByNormalizedRoleNameAsync(normalizedRoleName);
            if (role == null)
            {
                return false;
            }

            return user.Roles.Any(r => r.RoleId == role.Id);
        }

        public virtual async Task<IList<User>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Throw.IfArgumentNull(normalizedRoleName, nameof(normalizedRoleName));

            var role = await _roleProvider.FindByNormalizedRoleNameAsync(normalizedRoleName);

            if (role == null)
            {
                return new List<User>();
            }

            return await _unitOfWorkManager.PerformAsyncUow<Task<IList<User>>>(async () =>
            {
                var query = from userRole in _userRoleRepository.GetAll()
                    join user in _userRepository.GetAll() on userRole.UserId equals user.Id
                    select user;
                return await Task.FromResult(query.ToList());
            });
        }
        #endregion
    }
}