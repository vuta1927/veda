using System;
using VDS.AspNetCore.Mvc;
using VDS.AspNetCore.Mvc.Antiforgery;
using VDS.AspNetCore.Mvc.Security;
using VDS.AspNetCore.Security.AntiForgery;
using VDS.Configuration;
using VDS.Dependency;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace VDS.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Integrates to AspNet Core
        /// </summary>
        /// <param name="services">Services</param>
        /// <returns></returns>
        public static IServiceProvider AddDomain(this IServiceCollection services)
        {
            return services.AddDomain(options => { });
        }

        /// <summary>
        /// Integrates to AspNet Core
        /// </summary>
        /// <param name="services">Services</param>
        /// <param name="optionsAction">An action to get/modify config</param>
        /// <returns></returns>
        public static IServiceProvider AddDomain(this IServiceCollection services, Action<IConfigure> optionsAction)
        {
            var options = Configure.New(services);

            optionsAction(options);
            ConfigureAspNetCore(services);

            options.Initialize();

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.Using<IOptions<AntiforgeryOptions>>(o =>
            {
                o.Value.HeaderName = serviceProvider.GetService<IAntiForgeryConfiguration>().TokenHeaderName;
            });
            return serviceProvider;
        }

        private static void ConfigureAspNetCore(IServiceCollection services)
        {
            //See https://github.com/aspnet/Mvc/issues/3936 to know why we added these services.
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //Use DI to create controllers
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());

            //Use DI to create view components
            services.Replace(ServiceDescriptor.Singleton<IViewComponentActivator, ServiceBasedViewComponentActivator>());

            //Change anti forgery filters (to work proper with non-browser clients)
            services.Replace(ServiceDescriptor.Transient<AutoValidateAntiforgeryTokenAuthorizationFilter, DomainAutoValidateAntiforgeryTokenAuthorizationFilter>());
            services.Replace(ServiceDescriptor.Transient<ValidateAntiforgeryTokenAuthorizationFilter, DomainValidateAntiforgeryTokenAuthorizationFilter>());

            services.Replace(ServiceDescriptor.Singleton<IAntiForgeryConfiguration, AntiForgeryConfiguration>());
            services.Replace(ServiceDescriptor.Transient<IAntiForgeryManager, AspNetCoreAntiForgeryManager>());

            //Configure MVC
            services.Configure<MvcOptions>(mvcOptions =>
            {
                mvcOptions.AddDomain(services);
            });

            services.AddSingleton<IMemoryCache, MemoryCache>();

            // LocalCache is registered as transient as its implementation resolves IMemoryCache, thus
            // there is no state to keep in its instance.
            services.AddTransient<IDistributedCache, MemoryDistributedCache>();

            services.AddSecurity();
        }
    }
}