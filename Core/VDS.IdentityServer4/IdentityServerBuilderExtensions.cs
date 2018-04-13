using System;
using System.IdentityModel.Tokens.Jwt;
using VDS.Security;
using IdentityModel;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.IdentityServer4
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddAppIdentityServer(this IIdentityServerBuilder builder, Action<AppIdentityServerOptions> optionsAction = null)
        {
            var options = new AppIdentityServerOptions();
            optionsAction?.Invoke(options);


            builder.AddAspNetIdentity<User>();

            builder.AddProfileService<AppProfileService>();
            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();

            if (options.UpdateAppClaimTypes)
            {
                AppClaimTypes.UserId = JwtClaimTypes.Subject;
                AppClaimTypes.UserName = JwtClaimTypes.Name;
                AppClaimTypes.Role = JwtClaimTypes.Role;
            }

            if (options.UpdateJwtSecurityTokenHandlerDefaultInboundClaimTypeMap)
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[AppClaimTypes.UserId] = AppClaimTypes.UserId;
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[AppClaimTypes.UserName] = AppClaimTypes.UserName;
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[AppClaimTypes.Role] = AppClaimTypes.Role;
            }

            return builder;
        }
    }
}
