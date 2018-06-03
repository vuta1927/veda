//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Timers;
//using ApiServer.Hubs;
//using ApiServer.Model;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;

//namespace ApiServer.Core.Queues
//{
//    public class ImageQueueService:IImageQueueService
//    {
//        private readonly VdsContext _context;
//        private readonly IHubContext<VdsHub> _hubContext;
//        private IMemoryCache _cache;

//        public static Dictionary<Guid, ConcurrentQueue<long>> ProjectDictionary = new Dictionary<Guid, ConcurrentQueue<long>>();
//        public static Dictionary<Guid, List<ImageForQueue>> ImageStorage = new Dictionary<Guid, List<ImageForQueue>>();

//        public static Dictionary<Guid, List<ImageForQueue>> ImagesUsed = new Dictionary<Guid, List<ImageForQueue>>();

//        public static Dictionary<ImageForQueue, Timer> MonitorTimers = new Dictionary<ImageForQueue, Timer>();

//        public ImageQueueService(VdsContext vdsContext, IHubContext<VdsHub> hubContext, IMemoryCache memoryCache)
//        {
//            _context = vdsContext;
//            _hubContext = hubContext;
//            _cache = memoryCache;
//        }

//        public void Append(long userId, Guid projectId)
//        {
//            var queues = RuntimeDataContainer.ProjectDictionary;
//            if (queues.ContainsKey(projectId))
//            {
//                var queue = new ConcurrentQueue<long>();

//                if (queues.TryGetValue(projectId, out queue))
//                {
//                    queues[projectId].Enqueue(userId);
//                }
//            }
//            else
//            {
//                var queue = new ConcurrentQueue<long>();

//                queues.Add(projectId, queue);

//                queues[projectId].Enqueue(userId);

//                var imageIds = _context.Images.Where(x => x.Project.Id == projectId).Select(x => x.Id).ToList();

//                var imageForQueues = new List<ImageForQueue>();

//                foreach (var imageId in imageIds)
//                {
//                    imageForQueues.Add(new ImageForQueue()
//                    {
//                        ImageId = imageId
//                    });
//                }

//                RuntimeDataContainer.ImageStorage.Add(projectId, imageForQueues);
//                RuntimeDataContainer.ImagesUsed.Add(projectId, new List<ImageForQueue>());
//            }
//        }

//        public async Task<Image> GetImage(Guid projectId, Guid imgToRelease, long usrId)
//        {
//            long userId;
//            Image image;

//            var hadTaken = RuntimeDataContainer.ImagesUsed[projectId];
//            var notTaken = RuntimeDataContainer.ImageStorage[projectId];

//            if (RuntimeDataContainer.ProjectDictionary[projectId].TryDequeue(out userId) && (userId == usrId))
//            {
//                if (imgToRelease.ToString().Equals("00000000-0000-0000-0000-000000000000"))
//                {
//                    var img = notTaken.FirstOrDefault();

//                    //img.TimeStart = DateTime.Now;

//                    image = await _context.Images.Include(x => x.QuantityCheck).Include(x => x.UsersTagged).Include(x => x.UserTaggedTimes).SingleOrDefaultAsync(m => m.Id == img.ImageId);

//                    var imgTaken = new ImageForQueue() { ImageId = img.ImageId, UserId = userId, LastPing = DateTime.Now };
//                    hadTaken.Add(imgTaken);

//                    notTaken.Remove(img);

//                    MonitorHeartbeat(projectId, imgTaken);

//                    if (image != null)
//                    {
//                        var imageTakent = new UserImageForHub() { ImageId = img.ImageId, UserName = _context.Users.SingleOrDefault(x => x.Id == usrId).UserName };

//                        await _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { imageTakent });
//                    }

//                    return image;
//                }
//                else
//                {
//                    var imgToRemoveInDict = hadTaken.FirstOrDefault(x => x.ImageId == imgToRelease);
//                    if (imgToRemoveInDict != null)
//                    {
//                        hadTaken.Remove(imgToRemoveInDict);
//                    }

//                    var imgToTake = notTaken.FirstOrDefault();

//                    image = await _context.Images.Include(x => x.QuantityCheck).Include(x => x.UsersTagged).Include(x => x.UserTaggedTimes).SingleOrDefaultAsync(m => m.Id == imgToTake.ImageId);

//                    var imgTaken = new ImageForQueue() { ImageId = image.Id, UserId = userId, LastPing = DateTime.Now };

//                    hadTaken.Add(imgTaken);

//                    notTaken.Remove(imgToTake);

//                    notTaken.Add(new ImageForQueue() { ImageId = imgToRelease });

//                    MonitorHeartbeat(projectId, imgTaken);

//                    var a = new List<UserImageForHub>();
//                    a.Add(new UserImageForHub() { ImageId = imgToRelease, UserName = null });
//                    a.Add(new UserImageForHub() { ImageId = imgTaken.ImageId, UserName = _context.Users.SingleOrDefault(x => x.Id == usrId).UserName });
//                    await _hubContext.Clients.All.SendAsync("userUsingInfo", a);


//                    return image;
//                }

//            }
//            else
//            {
//                await GetImage(projectId, imgToRelease, usrId);
//            }
//            return null;
//        }

//        public async Task<Image> GetImageById(Guid projectId, Guid ImgId, long usrId)
//        {
//            Image image;

//            var hadTaken = RuntimeDataContainer.ImagesUsed[projectId];
//            var notTaken = RuntimeDataContainer.ImageStorage[projectId];
//            if (RuntimeDataContainer.ProjectDictionary[projectId].TryDequeue(out usrId))
//            {
//                if (hadTaken.Any(x => x.ImageId == ImgId))
//                {
//                    return null;
//                }

//                if (notTaken.Any(x => x.ImageId == ImgId))
//                {
//                    var img = notTaken.SingleOrDefault(x => x.ImageId == ImgId);

//                    image = await _context.Images.Include(x => x.QuantityCheck).Include(x => x.UserTaggedTimes).SingleOrDefaultAsync(m => m.Id == ImgId);

//                    var imgTaken = new ImageForQueue() { ImageId = image.Id, LastPing = DateTime.Now, UserId = usrId };

//                    hadTaken.Add(imgTaken);

//                    notTaken.Remove(img);

//                    MonitorHeartbeat(projectId, imgTaken);

//                    if (image != null)
//                    {
//                        var userImageForHub = new UserImageForHub() { ImageId = imgTaken.ImageId, UserName = _context.Users.SingleOrDefault(x => x.Id == usrId).UserName };

//                        await _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { userImageForHub });
//                    }

//                    return image;
//                }
//            }
//            return null;
//        }

//        public void AddImage(Guid projectId, Guid imageId)
//        {
//            if (RuntimeDataContainer.ImageStorage.ContainsKey(projectId))
//            {
//                RuntimeDataContainer.ImageStorage[projectId].Add(new ImageForQueue()
//                {
//                    ImageId = imageId
//                });
//            }
//        }
//        public async Task DeleteImage(Guid projectId, Guid imageId)
//        {
//            if (!RuntimeDataContainer.ImagesUsed.ContainsKey(projectId)) return;

//            if (RuntimeDataContainer.ImagesUsed[projectId].Any(x => x.ImageId == imageId))
//            {
//                try
//                {
//                    await ReleaseImage(projectId, imageId);
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }

//            }
//            if (RuntimeDataContainer.ImageStorage[projectId].Any(x => x.ImageId == imageId))
//            {
//                var item = RuntimeDataContainer.ImageStorage[projectId].SingleOrDefault(x => x.ImageId == imageId);
//                RuntimeDataContainer.ImageStorage[projectId].Remove(item);
//            }
//        }

//        public async Task<bool> ReleaseImage(Guid projectId, Guid imgId)
//        {
//            var img = RuntimeDataContainer.ImagesUsed[projectId].FirstOrDefault(x => x.ImageId == imgId);
//            if (img != null && RuntimeDataContainer.ImagesUsed[projectId].Contains(img))
//            {
//                var a = new UserImageForHub() { UserName = null, ImageId = imgId };

//                //await UpdateTaggedTime(img.TimeStart, img.ImageId);

//                RuntimeDataContainer.ImagesUsed[projectId].Remove(img);
//                img.LastPing = new DateTime();
//                RuntimeDataContainer.ImageStorage[projectId].Add(img);

//                await _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { a });

//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        private void MonitorHeartbeat(Guid projectId, ImageForQueue ImageToMonitor)
//        {
//            if (RuntimeDataContainer.MonitorTimers.ContainsKey(ImageToMonitor))
//            {
//                RuntimeDataContainer.MonitorTimers.Remove(ImageToMonitor);
//            }

//            Timer mTimer = new Timer();
//            mTimer.Elapsed += delegate { CheckHeartbeat(projectId, ImageToMonitor); };
//            mTimer.Interval = 10000;
//            mTimer.Enabled = true;
//            RuntimeDataContainer.MonitorTimers.Add(ImageToMonitor, mTimer);
//        }

//        public void SetTimePing(Guid projectId, Guid imageId, DateTime pingTime)
//        {
//            var img = RuntimeDataContainer.ImagesUsed[projectId].FirstOrDefault(x => x.ImageId == imageId);
//            if (img != null)
//            {
//                img.LastPing = pingTime;
//            }
//        }

//        private async Task CheckHeartbeat(Guid projectId, ImageForQueue image)
//        {
//            var timeOut = DateTime.Now;
//            var lastPing = image.LastPing;
//            var elapsedRunTime = (timeOut - lastPing).TotalSeconds;
//            if (elapsedRunTime >= 10)
//            {
//                if (RuntimeDataContainer.MonitorTimers.ContainsKey(image))
//                {
//                    RuntimeDataContainer.MonitorTimers[image].Enabled = false;
//                    RuntimeDataContainer.MonitorTimers.Remove(image);
//                    await ReleaseImage(projectId, image.ImageId);
//                }
//            }
//        }

//        public string GetUserUsing(Guid projectId, Guid ImageId, VdsContext context)
//        {
//            var result = new List<UserImageForHub>();
//            if (RuntimeDataContainer.ImagesUsed.ContainsKey(projectId))
//            {
//                foreach (var data in RuntimeDataContainer.ImagesUsed[projectId])
//                {
//                    if (data.ImageId == ImageId)
//                        return context.Users.FirstOrDefault(x => x.Id == data.UserId).UserName;
//                }
//            }

//            return null;
//        }
//    }
//}
