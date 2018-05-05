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

namespace ApiServer.Core.Queues
{
    public static class ImageQueues
    {
        private static Dictionary<Guid, ConcurrentQueue<long>> _queues = new Dictionary<Guid, ConcurrentQueue<long>>();

        private static Dictionary<Guid, List<ImageForQueue>> _image_notTaken = new Dictionary<Guid, List<ImageForQueue>>();

        private static Dictionary<Guid, List<ImageForQueue>> _image_Taken = new Dictionary<Guid, List<ImageForQueue>>();

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
                        ImageId = imageId,
                        UserId = userId
                    });
                }

                _image_notTaken.Add(projectId, imageForQueues);
                _image_Taken.Add(projectId, new List<ImageForQueue>());
            }
        }

        public static async Task<Image> GetImage(Guid projectId, Guid imgToRelease, long usrId)
        {
            long userId;
            Image image;

            var hadTaken = _image_Taken[projectId];
            var notTaken = _image_notTaken[projectId];

            if (_queues[projectId].TryDequeue(out userId))
            {
                if (imgToRelease.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                {
                    var img = _image_notTaken[projectId].FirstOrDefault();

                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == img.ImageId);

                    _image_Taken[projectId].Add(img);

                    _image_notTaken[projectId].Remove(img);

                    MonitorHeartbeat(projectId, img);

                    if (image != null)
                    {
                        var ImageTakent = GetListImagesTaken(projectId);

                        await _hubContext.Clients.All.SendAsync("userUsing", ImageTakent);
                    }

                    return image;
                }
                else
                {
                    _image_notTaken[projectId].Add(new ImageForQueue() { ImageId = imgToRelease, UserId = userId });
                    var imageRelease = new ImageForQueue();
                    var imgToRemoveInDict = _image_Taken[projectId].FirstOrDefault(x => x.ImageId == imgToRelease);
                    if (imgToRemoveInDict != null)
                    {
                        _image_Taken[projectId].Remove(imgToRemoveInDict);

                        imageRelease = new ImageForQueue() { UserId = imgToRemoveInDict.UserId, ImageId = imgToRemoveInDict.ImageId };
                    }
                    var img = _image_notTaken[projectId].FirstOrDefault();

                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == img.ImageId);

                    img.LastPing = DateTime.Now;

                    _image_Taken[projectId].Add(img);

                    _image_notTaken[projectId].Remove(img);

                    MonitorHeartbeat(projectId, img);

                    if (image != null)
                    {
                        var ImageTakent = GetListImagesTaken(projectId);

                        await _hubContext.Clients.All.SendAsync("userUsing", ImageTakent);

                        await _hubContext.Clients.All.SendAsync("userRelease", imageRelease);
                    }

                    return image;
                }

            }
            return null;
        }

        public static async Task<Image> GetImageById(Guid projectId, Guid ImgId, long usrId)
        {
            long userId;
            Image image;

            if (_queues[projectId].TryDequeue(out userId))
            {
                ImageForQueue img;

                if (_image_Taken[projectId].Any(x => x.ImageId == ImgId && x.UserId == usrId))
                {
                    img = _image_Taken[projectId].FirstOrDefault(x => x.ImageId == ImgId && x.UserId == usrId);
                }
                else
                {
                    img = _image_notTaken[projectId].FirstOrDefault(x => x.ImageId == ImgId);
                }

                if (img != null && _image_notTaken[projectId].Contains(img))
                {
                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == ImgId);

                    img.LastPing = DateTime.Now;

                    _image_Taken[projectId].Add(img);

                    _image_notTaken[projectId].Remove(img);

                    MonitorHeartbeat(projectId, img);

                    if (image != null)
                    {
                        var ImageTakent = GetListImagesTaken(projectId);

                        await _hubContext.Clients.All.SendAsync("userUsing", ImageTakent);
                    }

                    return image;
                }
            }
            return null;
        }

        public static async Task<bool> ReleaseImage(Guid projectId, Guid imgId)
        {
            var img = _image_Taken[projectId].FirstOrDefault(x => x.ImageId == imgId);
            if (img != null && _image_Taken[projectId].Contains(img))
            {
                var a = new UserImageForHub() { UserId = img.UserId, ImageId = imgId };

                _image_Taken[projectId].Remove(img);
                img.LastPing = new DateTime();
                _image_notTaken[projectId].Add(img);

                await _hubContext.Clients.All.SendAsync("userRelease", a);

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
            var img = _image_Taken[projectId].SingleOrDefault(x => x.ImageId == imageId);
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

        public static List<UserImageForHub> GetListImagesTaken(Guid projectId)
        {
            var result = new List<UserImageForHub>();
            if (_image_Taken.ContainsKey(projectId))
            {
                foreach (var data in _image_Taken[projectId])
                {
                    result.Add( new UserImageForHub() { ImageId = data.ImageId, UserId = data.UserId });
                }
            }

            return result;
        }
    }
}
