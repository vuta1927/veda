using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using ApiServer.Model;

namespace ApiServer.BackgroundJobs
{
    public class ImageQueueJobs
    {
        private readonly VdsContext _ctx;
        public ImageQueueJobs(VdsContext context)
        {
            _ctx = context;
        }

        public void Clean()
        {
            _ctx.imageQueues.RemoveRange(_ctx.imageQueues);
        }

        public void CheckTimeOut(Guid imageId, long userId)
        {
            var currentTime = DateTime.Now;
            var data = _ctx.imageQueues.Where(x=>x.ImageId == imageId && x.UserId == userId);

            if(data.Count() > 0)
            {
                foreach (var row in data)
                {
                    var t = DateTime.Now.Subtract(row.LastPing).TotalMinutes;
                    if(t >= 30)
                    {
                        try
                        {
                            _ctx.imageQueues.Remove(row);
                            _ctx.SaveChanges();
                        }catch(Exception e)
                        {
                            Console.WriteLine("[*] BackgroundJob Error: " + e.ToString());
                        }
                    }
                }
            }
        }
    }
}
