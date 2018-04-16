using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using VDS.AspNetCore;
using VDS.Helpers.Extensions;
using VDS.Storage.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ApiServer.Model;
using IdentityServer4.Services;
using ApiServer.Core.Authorization;
using ApiServer.Controllers;
using ApiServer.Controllers.Auth;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace ApiServer
{
    public class Startup
    {
        private static string _defaultCorsPolicyName = "http://localhost:4500";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(
                options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddDomain(options =>
            {
                options.DefaultNameOrConnectionString = Configuration.GetConnectionString("Default");
                options.BackgroundJobs.IsJobExecutionEnabled = false;
                // Configure storage
                options.Storage.UseEntityFrameworkCore(c =>
                {
                    c.AddDbContext<VdsContext>(config =>
                            config.DbContextOptions.UseSqlServer(Configuration.GetConnectionString("Default")));
                });
            });

            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<PermissionsController>();
            services.AddTransient<RolesController>();
            services.AddTransient<UsersController>();
            services.AddTransient<ProjectsController>();
            services.AddTransient<ImagesController>();
            services.AddTransient<TagsController>();
            services.AddTransient<QuantityChecksController>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opts =>
            {
                opts.Authority = Configuration["IdentityServer:Authority"];
                opts.RequireHttpsMetadata = false;
                opts.Audience = "vds-api";
            });

            // Configure CORS for angular5 UI
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            Configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePreFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                )
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(_defaultCorsPolicyName);

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
