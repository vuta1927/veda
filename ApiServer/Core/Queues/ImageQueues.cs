using ApiServer.Hubs;
using ApiServer.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static ApiServer.Model.views.ImageModel;

namespace ApiServer.Core.Queues
{
    public static class ImageQueues
    {
        private static IDictionary<Guid, ConcurrentQueue<long>> _queues = new Dictionary<Guid, ConcurrentQueue<long>>();

        private static IDictionary<Guid, List<ImageForQueue>> _imageStorage = new Dictionary<Guid, List<ImageForQueue>>();

        private static IDictionary<Guid, List<ImageForQueue>> _imagesUsed = new Dictionary<Guid, List<ImageForQueue>>();

        private static Dictionary<ImageForQueue, Timer> _monitorTimer = new Dictionary<ImageForQueue, Timer>();

        private static VdsContext _context;

        private static IHubContext<VdsHub> _hubContext;

        public static ConcurrentQueue<long> GetQueue(Guid projectId)
        {
            var result = new ConcurrentQueue<long>();
            _queues.TryGetValue(projectId, out result);
            return result;
        }

        public static void Append(long userId, Guid projectId, VdsContext context, IHubContext<VdsHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;

            if (_queues.ContainsKey(projectId))
            {
                var queue = new ConcurrentQueue<long>();

                if (_queues.TryGetValue(projectId, out queue))
                {
                    _queues[projectId].Enqueue(userId);
                }
            }
            else
            {
                var queue = new ConcurrentQueue<long>();

                _queues.Add(projectId, queue);

                _queues[projectId].Enqueue(userId);

                var imageIds = _context.Images.Where(x => x.Project.Id == projectId).Select(x => x.Id).ToList();

                var imageForQueues = new List<ImageForQueue>();

                foreach (var imageId in imageIds)
                {
                    imageForQueues.Add(new ImageForQueue()
                    {
                        ImageId = imageId
                    });
                }

                _imageStorage.Add(projectId, imageForQueues);
                _imagesUsed.Add(projectId, new List<ImageForQueue>());
            }
        }

        public static async Task<Image> GetImage(Guid projectId, Guid imgToRelease, long usrId)
        {
            long userId;
            Image image;

            var hadTaken = _imagesUsed[projectId];
            var notTaken = _imageStorage[projectId];

            if (_queues[projectId].TryDequeue(out userId) && (userId == usrId))
            {
                if (imgToRelease.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                {
                    var img = _imageStorage[projectId].FirstOrDefault();

                    //img.TimeStart = DateTime.Now;

                    image = await _context.Images.Include(x => x.QuantityCheck).Include(x => x.UsersTagged).Include(x => x.UserTaggedTimes).SingleOrDefaultAsync(m => m.Id == img.ImageId);

                    var imgTaken = new ImageForQueue() { ImageId = img.ImageId, UserId = userId, LastPing = DateTime.Now };
                    _imagesUsed[projectId].Add(imgTaken);

                    _imageStorage[projectId].Remove(img);

                    MonitorHeartbeat(projectId, imgTaken);

                    if (image != null)
                    {
                        var imageTakent = new UserImageForHub() { ImageId = img.ImageId, UserName = _context.Users.SingleOrDefault(x => x.Id == usrId).UserName };

                        await _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { imageTakent });
                    }

                    return image;
                }
                else
                {
                    var imgToRemoveInDict = _imagesUsed[projectId].FirstOrDefault(x => x.ImageId == imgToRelease);
                    if (imgToRemoveInDict != null)
                    {
                        _imagesUsed[projectId].Remove(imgToRemoveInDict);
                    }

                    var imgToTake = _imageStorage[projectId].FirstOrDefault();

                    image = await _context.Images.Include(x => x.QuantityCheck).Include(x => x.UsersTagged).Include(x => x.UserTaggedTimes).SingleOrDefaultAsync(m => m.Id == imgToTake.ImageId);

                    var imgTaken = new ImageForQueue() { ImageId = image.Id, UserId = userId, LastPing = DateTime.Now };

                    _imagesUsed[projectId].Add(imgTaken);

                    _imageStorage[projectId].Remove(imgToTake);

                    _imageStorage[projectId].Add(new ImageForQueue() { ImageId = imgToRelease });

                    MonitorHeartbeat(projectId, imgTaken);

                    var a = new List<UserImageForHub>();
                    a.Add(new UserImageForHub() { ImageId = imgToRelease, UserName = null });
                    a.Add(new UserImageForHub() { ImageId = imgTaken.ImageId, UserName = _context.Users.SingleOrDefault(x => x.Id == usrId).UserName });
                    await _hubContext.Clients.All.SendAsync("userUsingInfo", a);


                    return image;
                }

            }
            else
            {
                await GetImage(projectId, imgToRelease, usrId);
            }
            return null;
        }

        public static async Task<Image> GetImageById(Guid projectId, Guid ImgId, long usrId)
        {
            Image image;

            if (_queues[projectId].TryDequeue(out usrId))
            {
                if (_imagesUsed[projectId].Any(x => x.ImageId == ImgId))
                {
                    return null;
                }

                if (_imageStorage[projectId].Any(x => x.ImageId == ImgId))
                {
                    var img = _imageStorage[projectId].SingleOrDefault(x => x.ImageId == ImgId);

                    image = await _context.Images.Include(x => x.QuantityCheck).Include(x => x.UserTaggedTimes).SingleOrDefaultAsync(m => m.Id == ImgId);

                    var imgTaken = new ImageForQueue() { ImageId = image.Id, LastPing = DateTime.Now, UserId = usrId };

                    _imagesUsed[projectId].Add(imgTaken);

                    _imageStorage[projectId].Remove(img);

                    MonitorHeartbeat(projectId, imgTaken);

                    if (image != null)
                    {
                        var userImageForHub = new UserImageForHub() { ImageId = imgTaken.ImageId, UserName = _context.Users.SingleOrDefault(x => x.Id == usrId).UserName };

                        await _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { userImageForHub });
                    }

                    return image;
                }
            }
            return null;
        }

        public static void AddImage(Guid projectId, Guid imageId)
        {
            if (_imageStorage.ContainsKey(projectId))
            {
                _imageStorage[projectId].Add(new ImageForQueue()
                {
                    ImageId = imageId
                });
            }
        }
        public static async Task DeleteImage(Guid projectId, Guid imageId)
        {
            if (!_imagesUsed.ContainsKey(projectId)) return;

            if (_imagesUsed[projectId].Any(x => x.ImageId == imageId))
            {
                try
                {
                    await ReleaseImage(projectId, imageId);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            if (_imageStorage[projectId].Any(x => x.ImageId == imageId))
            {
                var item = _imageStorage[projectId].SingleOrDefault(x => x.ImageId == imageId);
                _imageStorage[projectId].Remove(item);
            }
        }

        public static async Task<bool> ReleaseImage(Guid projectId, Guid imgId)
        {
            var img = _imagesUsed[projectId].FirstOrDefault(x => x.ImageId == imgId);
            if (img != null && _imagesUsed[projectId].Contains(img))
            {
                var a = new UserImageForHub() { UserName = null, ImageId = imgId };

                //await UpdateTaggedTime(img.TimeStart, img.ImageId);

                _imagesUsed[projectId].Remove(img);
                img.LastPing = new DateTime();
                _imageStorage[projectId].Add(img);

                await _hubContext.Clients.All.SendAsync("userUsingInfo", new object[] { a });

                return true;
            }
            else
            {
                return false;
            }
        }

        private static void MonitorHeartbeat(Guid projectId, ImageForQueue ImageToMonitor)
        {
            if (_monitorTimer.ContainsKey(ImageToMonitor))
            {
                _monitorTimer.Remove(ImageToMonitor);
            }

            Timer mTimer = new Timer();
            mTimer.Elapsed += delegate { CheckHeartbeat(projectId, ImageToMonitor); };
            mTimer.Interval = 10000;
            mTimer.Enabled = true;
            _monitorTimer.Add(ImageToMonitor, mTimer);
        }

        public static void SetTimePing(Guid projectId, Guid imageId, DateTime pingTime)
        {
            var img = _imagesUsed[projectId].FirstOrDefault(x => x.ImageId == imageId);
            if (img != null)
            {
                img.LastPing = pingTime;
            }
        }

        private static async Task CheckHeartbeat(Guid projectId, ImageForQueue image)
        {
            var timeOut = DateTime.Now;
            var lastPing = image.LastPing;
            var elapsedRunTime = (timeOut - lastPing).TotalSeconds;
            if (elapsedRunTime >= 10)
            {
                if (_monitorTimer.ContainsKey(image))
                {
                    _monitorTimer[image].Enabled = false;
                    _monitorTimer.Remove(image);
                    await ReleaseImage(projectId, image.ImageId);
                }
            }
        }

        public static string GetUserUsing(Guid projectId, Guid ImageId, VdsContext context)
        {
            var result = new List<UserImageForHub>();
            if (_imagesUsed.ContainsKey(projectId))
            {
                foreach (var data in _imagesUsed[projectId])
                {
                    if (data.ImageId == ImageId)
                        return context.Users.FirstOrDefault(x => x.Id == data.UserId).UserName;
                }
            }

            return null;
        }
    }
}
