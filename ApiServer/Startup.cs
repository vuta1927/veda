﻿using System;
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
using ApiServer.Core.Authorization;
using ApiServer.Controllers;
using ApiServer.Controllers.Auth;
using ApiServer.Hubs;
using ApiServer.Core.Merge;
using ApiServer.Core.Email;
using ApiServer.Core.Queues;
using Hangfire;
using Microsoft.AspNetCore.Http.Features;

namespace ApiServer
{
    public class Startup
    {
        private static string _defaultCorsPolicyName = "http://localhost:52000";

        public static IServiceProvider Provider { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire")));
            services.AddMemoryCache();
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });
            services.AddMvc()
                .AddJsonOptions(
                options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            //services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("HangFireDB")));

            services.AddDomain(options =>
            {
                options.DefaultNameOrConnectionString = Configuration.GetConnectionString("Default");
                // Configure storage
                options.Storage.UseEntityFrameworkCore(c =>
                {
                    c.AddDbContext<VdsContext>(config =>
                            config.DbContextOptions.UseSqlServer(Configuration.GetConnectionString("Default")));
                });
            });

            services.AddTransient<PermissionsController>();
            services.AddTransient<RolesController>();
            services.AddTransient<UsersController>();
            services.AddTransient<ProjectsController>();
            services.AddTransient<ProjectUsersController>();
            services.AddTransient<ImagesController>();
            services.AddTransient<TagsController>();
            services.AddTransient<ClassesController>();
            services.AddTransient<QuantityChecksController>();
            services.AddTransient<MergeController>();
            services.AddTransient<ImportProjectController>();
            services.AddTransient<ExportProjectController>();
            services.AddTransient<ProjectSettingsController>();
            services.AddTransient<DashboardController>();
            services.AddTransient<UserProfileController>();

            services.AddScoped<IImageQueueService, ImageQueueService>();
            services.AddScoped<IMergeService, MergeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailHelper, EmailHelper>();

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
                        .AllowAnyOrigin()
                        .AllowCredentials()
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

            app.UseHangfireServer();

            app.UseHangfireDashboard();

            app.UseSignalR(routes =>
            {
                routes.MapHub<VdsHub>("/hubs/image");
            });

            Provider = app.ApplicationServices;

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
