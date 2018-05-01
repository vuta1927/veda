﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ApiServer.Model;
using Hangfire;

namespace ApiServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            
            //RecurringJob.AddOrUpdate(() => Console.WriteLine("hang file job."), Cron.Minutely);
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseKestrel()
            .UseUrls("http://localhost:52719")
            .UseStartup<Startup>()
            .Build();
    }
}
