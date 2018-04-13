using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using VDS.AspNetCore.Mvc.Extensions;
using VDS.AspNetCore.Mvc.Models;
using VDS.AspNetCore.Mvc.Results;
using VDS.AspNetCore.Mvc.Security;
using VDS.Helpers.Extensions;
using VDS.Reflection;
using VDS.Security;
using VDS.Security.Permissions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VDS.AspNetCore.Mvc.Authorization
{
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IErrorInfoBuilder _errorInfoBuilder;
        private readonly IPermissionProviderService _permissionProviderService;
        
        public AuthorizationFilter(
            IAuthorizationService authorizationService,
            IErrorInfoBuilder errorInfoBuilder,
            IPermissionProviderService permissionProviderService)
        {
            _authorizationService = authorizationService;
            _errorInfoBuilder = errorInfoBuilder;
            _permissionProviderService = permissionProviderService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Allow Anonymous skips all authorization
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                return;
            }

            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            var authorizationAttributes = GetAuthorizeAttributes(context.ActionDescriptor.GetMethodInfo(),
                context.ActionDescriptor.GetMethodInfo().DeclaringType);

            if (!authorizationAttributes.IsNullOrEmpty())
            {
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    HandlerUnAuthorization(context, new SecurityException("Current user did not login to the application"));
                    return;
                }
            }

            foreach (var customAuthorizeAttribute in authorizationAttributes)
            {
                var permissions = new List<Permission>();
                foreach (var permission in customAuthorizeAttribute.Permissions)
                {
                    if (_permissionProviderService.GetPermissionBy(permission) == null)
                    {
                        HandlerUnAuthorization(context, new SecurityException("No permission named '{0}' exists.".FormatWith(permission)));
                        return;
                    }
                    permissions.Add(_permissionProviderService.GetPermissionBy(permission));
                }

                if (!await CheckAccess(context.HttpContext.User, customAuthorizeAttribute.RequireAllPermissions, permissions.ToArray()))
                {
                    if (customAuthorizeAttribute.RequireAllPermissions)
                    {
                        HandlerUnAuthorization(context, new SecurityException("Required permissions are not granted. All of these permissions must be granted: {0}".FormatWith(string.Join(", ", permissions.Select(x => x.Name)))));
                        return;
                    }

                    HandlerUnAuthorization(context, new SecurityException("Required permissions are not granted. At least one of these permissions must be granted: {0}".FormatWith(string.Join(", ", permissions.Select(x => x.Name)))));
                    return;
                }
            }
        }

        private async Task<bool> CheckAccess(ClaimsPrincipal user, bool requiredAll, Permission[] permissions)
        {
            if (permissions.IsNullOrEmpty())
            {
                return true;
            }

            if (requiredAll)
            {
                foreach (var permission in permissions)
                {
                    if (!await _authorizationService.AuthorizeAsync(user, permission))
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (var permission in permissions)
            {
                if (await _authorizationService.AuthorizeAsync(user, permission))
                {
                    return true;
                }
            }

            return false;
        }

        private void HandlerUnAuthorization(AuthorizationFilterContext context, SecurityException ex)
        {
            if (ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
            {
                context.Result = new ObjectResult(new AjaxResponse(_errorInfoBuilder.BuildForException(ex), true))
                {
                    StatusCode = context.HttpContext.User.Identity.IsAuthenticated
                        ? (int)System.Net.HttpStatusCode.Forbidden
                        : (int)System.Net.HttpStatusCode.Unauthorized
                };
            }
            else
            {
                context.Result = new ChallengeResult();
            }
        }

        [NotNull]
        public AppAuthorizeAttribute[] GetAuthorizeAttributes(MethodInfo methodInfo, Type type)
        {

            var authorizeAttributes =
                ReflectionHelper
                .GetAttributesOfMemberAndType(methodInfo, type)
                .OfType<AppAuthorizeAttribute>()
                .ToArray();

            if (!authorizeAttributes.Any())
            {
                return new AppAuthorizeAttribute[] {};
            }

            return authorizeAttributes;
        }
    }
}