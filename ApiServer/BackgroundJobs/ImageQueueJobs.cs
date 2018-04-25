using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using ApiServer.Model;

namespace ApiServer.BackgroundJobs
{
    public static class ImageQueueJobs
    {
        public static void Clean(VdsContext _ctx)
        {
            _ctx.imageQueues.RemoveRange(_ctx.imageQueues);
        }

        public static void CheckTimeOut(VdsContext _ctx)
        {
            var currentTime = DateTime.Now;
            var data = _ctx.imageQueues;

            if(data.Count() > 0)
            {
                foreach (var row in data)
                {
                    if (row.LastPing == null) return;
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
