using System;
using System.Linq;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VDS.AspNetCore;
using VDS.IdentityServer4;
using VDS.Storage.EntityFrameworkCore;
using VDS.Helpers.Extensions;
namespace AuthServer
{
    public class Startup
    {
        private static string _defaultCorsPolicyName = "http://localhost:52000";
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDomain(options =>
            {
                options.DefaultNameOrConnectionString = Configuration.GetConnectionString("Default");
                options.Storage.UseEntityFrameworkCore(opt =>
                {
                    opt.AddDbContext<VdsContext>(config => config.DbContextOptions.UseSqlServer(Configuration.GetConnectionString("Default")));
                });
            });

            services.AddIdentityServer(x => { x.IssuerUri = Configuration["IdentityServer:Authority"];})
            .AddDeveloperSigningCredential()
            .AddInMemoryIdentityResources(Config.GetIdentityResources())
            .AddInMemoryApiResources(Config.GetApiResources())
            .AddInMemoryClients(Config.GetClients())
            .AddAppIdentityServer();

            services.AddTransient<IProfileService, ProfileService>();

            // Configure CORS for angular5 UI
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                )
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseCors(_defaultCorsPolicyName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseIdentityServer();
        }
    }
}
