using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ApiServer.Core.SignalR;
using ApiServer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ApiServer.Hubs
{
    public class VdsHub: Hub
    {
        private readonly VdsContext _context;
        private IConfiguration Configuration;
        private Timer _timer;
        public VdsHub(VdsContext vdscontext, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _context = vdscontext;
            Configuration = configuration;
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
        }

        public override Task OnConnectedAsync()
        {
            _timer = new Timer(5000);
            _timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            _timer.AutoReset = true;
            _timer.Enabled = true;
            return base.OnConnectedAsync();
        }

        protected void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            try
            {
                //var imgQueue = _context.imageQueues.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
                //if (imgQueue != null)
                //{
                //    ImageRelease(imgQueue.ImageId);
                //}
            }
            catch (Exception)
            {

            }
            finally{
                _timer.Start();
            }
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            //var imgQueue = _context.imageQueues.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
            //if (imgQueue != null)
            //{
            //    ImageRelease(imgQueue.ImageId);
            //}
            return base.OnDisconnectedAsync(exception);
        }
        
    }
}
