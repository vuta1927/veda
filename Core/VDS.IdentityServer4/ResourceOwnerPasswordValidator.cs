using System.Threading.Tasks;
using VDS.Security;
using IdentityModel;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace VDS.IdentityServer4
{
    public class ResourceOwnerPasswordValidator : ResourceOwnerPasswordValidator<User>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger _logger;

        public ResourceOwnerPasswordValidator(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEventService events,
            ILogger<ResourceOwnerPasswordValidator<User>> logger)
            : base(userManager, signInManager, events, logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public override async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, context.Password, true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Credentials validated for username: {username}", context.UserName);

                    var sub = await _userManager.GetUserIdAsync(user);
                    context.Result = new GrantValidationResult(sub, OidcConstants.AuthenticationMethods.Password);
                    return;
                }

                _logger.LogInformation(
                    result.IsLockedOut
                        ? "Authentication failed for username: {username}, reason: not allowed"
                        : "Authentication failed for username: {username}, reason: invalid credentials",
                    context.UserName);
            }
            else
            {
                _logger.LogInformation("No user found matching username: {username}", context.UserName);
            }
            
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
        }
    }
}