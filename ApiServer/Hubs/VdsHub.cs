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
                var imgQueue = _context.imageQueues.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (imgQueue != null)
                {
                    ImageRelease(imgQueue.ImageId);
                }
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
            var imgQueue = _context.imageQueues.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (imgQueue != null)
            {
                ImageRelease(imgQueue.ImageId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public void CurrentWorker(Guid projectId, Guid imageId, long userid)
        {
            if (!_context.Projects.Any(x => x.Id == projectId))
            {
                Clients.Caller.SendAsync("currentWorker", "Project not found !");
                return;
            }

            if (!_context.Images.Any(x => x.Id == imageId)) {
                Clients.Caller.SendAsync("currentWorker", "Image not found !");
                return;
            }

            var imgQueue = _context.imageQueues.SingleOrDefault(x => x.ImageId == imageId);

            if (imgQueue == null)
            {
                imgQueue = new ImageQueue()
                {
                    ImageId = imageId,
                    ProjectId = projectId,
                    UserId = userid,
                    ConnectionId = Context.ConnectionId
                };

                try
                {
                    _context.imageQueues.Add(imgQueue);
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    Clients.Caller.SendAsync("currentWorker", e.ToString());
                    return;
                }

                Clients.Caller.SendAsync("currentWorker", "");
                Clients.Others.SendAsync("userUsing", imgQueue);
            }
            else
            {
                Clients.Caller.SendAsync("currentWorker", imgQueue);
            }
        }

        public void ImageRelease(Guid imgId)
        {
            var imgQueue = _context.imageQueues.SingleOrDefault(x => x.ImageId == imgId);
            if (imgQueue == null)
            {
                Clients.Caller.SendAsync("imageRelease", "");
            }

            try
            {
                _context.imageQueues.Remove(imgQueue);
                _context.SaveChanges();
                Clients.Caller.SendAsync("imageRelease", "");
                Clients.Others.SendAsync("imageReleaseBroadcast", imgId);
            }
            catch (Exception e)
            {
                Clients.Caller.SendAsync("imageRelease", e.ToString());
            }
        }
    }
}
