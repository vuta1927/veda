using System;
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
                .UseStartup<Startup>()
                .UseUrls("http://191.168.0.108:52719")
                .Build();
    }
}
