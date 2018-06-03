using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ApiServer.Hubs;
using ApiServer.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VDS.Helpers.Extensions;

namespace ApiServer.Core.Queues
{
    public class ImageQueueService : IImageQueueService
    {
        private readonly VdsContext _context;
        private readonly IHubContext<VdsHub> _hubContext;
        private readonly object _locker = new object();
        private static Timer MonitorTimer = new Timer();
        public ImageQueueService(VdsContext vdsContext, IHubContext<VdsHub> hubContext)
        {
            _context = vdsContext;
            _hubContext = hubContext;
        }

        private void GenerateStorage(Guid projectId)
        {
            lock (_locker)
            {
                var storage = RuntimeDataContainer.ImageStorage;
                if (!storage.ContainsKey(projectId))
                {
                    var imgs = _context.Images.Include(x => x.Project).Where(x => x.Project.Id == projectId);
                    var imgQueues = new ConcurrentQueue<Guid>();
                    RuntimeDataContainer.ImageStorageOptional.Add(projectId, new List<Guid>());
                    var optionalStorage = RuntimeDataContainer.ImageStorageOptional;
                    foreach (var img in imgs)
                    {
                        imgQueues.Enqueue(img.Id);
                        optionalStorage[projectId].Add(img.Id);
                    }
                    storage.Add(projectId, imgQueues);
                    RuntimeDataContainer.ImagesUsed.Add(projectId, new List<ImageForQueue>());
                }
            }
        }

        public Image GetImage(Guid projectId, Guid imgToRelease, long usrId)
        {
            var image = new Image();
            lock (_locker)
            {
                if (!RuntimeDataContainer.ImageStorage.ContainsKey(projectId))
                {
                    GenerateStorage(projectId);
                }

                var queue = RuntimeDataContainer.ImageStorage[projectId];
                while (true)
                {
                    if (queue.TryDequeue(out var imageId))
                    {
                        if(RuntimeDataContainer.ImagesUsed[projectId].Any(x=>x.ImageId == imageId)) continue;

                        image = _context.Images.Include(x => x.QuantityCheck).Include(x => x.UsersTagged).Include(x => x.UserTaggedTimes).SingleOrDefault(m => m.Id == imageId);
                        if (image != null)
                        {
                            var i = RuntimeDataContainer.ImagesUsed[projectId]
                                .SingleOrDefault(x => x.ImageId == imgToRelease);
                            if (i != null)
                            {
                                RuntimeDataContainer.ImagesUsed[projectId].Remove(i);
                            }

                            if (!imgToRelease.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                            {
                                RuntimeDataContainer.ImageStorage[projectId].Enqueue(imgToRelease);
                                RuntimeDataContainer.ImageStorageOptional[projectId].Add(imgToRelease);
                            }

                            var im = new ImageForQueue()
                            {
                                ImageId = image.Id,
                                LastPing = DateTime.Now,
                                UserId = usrId
                            };

                            RuntimeDataContainer.ImagesUsed[projectId].Add(im);
                            RuntimeDataContainer.ImageStorageOptional[projectId].Remove(imageId);
                            MonitorIdle(projectId, RuntimeDataContainer.ImagesUsed[projectId].SingleOrDefault(x => x.ImageId == image.Id));

                            var a = new List<UserImageForHub>
                            {
                                new UserImageForHub() { ImageId = imgToRelease, UserName = null },
                                new UserImageForHub() { ImageId = im.ImageId, UserName = _context.Users.SingleOrDefault(x => x.Id == usrId).UserName }
                            };
                            _hubContext.Clients.All.SendAsync("userUsingInfo", a);
                        }
                        else
                        {
                            RuntimeDataContainer.ImageStorage[projectId].Enqueue(imageId);
                        }
                        break;
                    }
                }
                
            }
            return image;
        }

        public Image GetImageById(Guid projectId, Guid imgId, long usrId)
        {
            var image = new Image();
            var storage = RuntimeDataContainer.ImageStorageOptional;
            lock (_locker)
            {
                if (!storage.ContainsKey(projectId))
                {
                    GenerateStorage(projectId);
                }

                if (RuntimeDataContainer.ImagesUsed[projectId].Any(x => x.ImageId == imgId)) return image;


                if (!storage[projectId].Any()) return image;
                
                image = _context.Images.Include(x => x.QuantityCheck).Include(x => x.UsersTagged).Include(x => x.UserTaggedTimes).SingleOrDefault(m => m.Id == imgId);
                if (image != null)
                {
                    var i = new ImageForQueue() { ImageId = image.Id, LastPing = DateTime.Now, UserId = usrId };

                    RuntimeDataContainer.ImagesUsed[projectId].Add(i);
                    storage[projectId].Remove(imgId);
                    MonitorIdle(projectId, RuntimeDataContainer.ImagesUsed[projectId].SingleOrDefault(x=>x.ImageId == image.Id));
                    
                    _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { i });
                }

            }
            return image;
        }
        public void AddImage(Guid projectId, Guid imageId)
        {
            if (!RuntimeDataContainer.ImageStorage.ContainsKey(projectId)) return;
            if (!RuntimeDataContainer.ImageStorageOptional.ContainsKey(projectId)) return;
            RuntimeDataContainer.ImageStorage[projectId].Enqueue(imageId);
            RuntimeDataContainer.ImageStorageOptional[projectId].Add(imageId);
        }
        public async Task DeleteImage(Guid projectId, Guid imageId)
        {
            var isRelease = await ReleaseImage(projectId, imageId);  //for kick ass of any user using that fucking image ;)
            if (isRelease)
            {
                GetImageById(projectId, imageId, 0); // this imageId will be Dequeue ... it mean removed from queue :))
            }
        }

        public async Task<bool> ReleaseImage(Guid projectId, Guid imgId)
        {
            var img = RuntimeDataContainer.ImagesUsed[projectId].FirstOrDefault(x => x.ImageId == imgId);

            if (img == null) return false;

            RuntimeDataContainer.ImageStorage[projectId].Enqueue(imgId);
            RuntimeDataContainer.ImageStorageOptional[projectId].Add(imgId);
            RuntimeDataContainer.ImagesUsed[projectId].Remove(img);

            var a = new UserImageForHub() { UserName = null, ImageId = imgId };

            await _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { a });

            return true;
        }

        private void MonitorIdle(Guid projectId, ImageForQueue image)
        {
            MonitorTimer.Elapsed += delegate { CheckHeartbeat(projectId, image); };
            MonitorTimer.Interval = 10000;
            MonitorTimer.Enabled = true;
        }

        public void SetTimePing(Guid projectId, Guid imageId, DateTime pingTime)
        {
            if (!RuntimeDataContainer.ImagesUsed.ContainsKey(projectId)) return;
            var i = RuntimeDataContainer.ImagesUsed[projectId].SingleOrDefault(x => x.ImageId == imageId);
            if (i == null) return;
            i.LastPing = pingTime;
        }

        private async Task CheckHeartbeat(Guid projectId, ImageForQueue image)
        {
            var timeOut = DateTime.Now;
            var lastPing = image.LastPing;
            var elapsedRunTime = (timeOut - lastPing).TotalSeconds;
            if (elapsedRunTime >= 20)
            {
                MonitorTimer.Enabled = false;
                await ReleaseImage(projectId, image.ImageId);

            }
        }

        public async Task<string> GetUserUsing(Guid projectId, Guid imageId)
        {
            if (!RuntimeDataContainer.ImagesUsed.ContainsKey(projectId)) return null;
            var i = RuntimeDataContainer.ImagesUsed[projectId].SingleOrDefault(x => x.ImageId == imageId);
            if (i == null) return null;

            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == i.UserId);
            return user.UserName;
        }
    }
}
