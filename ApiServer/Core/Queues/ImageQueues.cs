using ApiServer.Model;
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

        public static ConcurrentQueue<long> GetQueue(Guid projectId)
        {
            var result = new ConcurrentQueue<long>();
            _queues.TryGetValue(projectId, out result);
            return result;
        }

        public static void Append(long userId, Guid projectId, VdsContext context)
        {
            _context = context;
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

                var ids = _context.Images.Where(x => x.Project.Id == projectId).Select(x => x.Id).ToList();

                var imageForQueues = new List<ImageForQueue>();

                foreach (var id in ids)
                {
                    imageForQueues.Add(new ImageForQueue()
                    {
                        Id = id
                    });
                }

                _image_notTaken.Add(projectId, imageForQueues);
                _image_Taken.Add(projectId, new List<ImageForQueue>());
            }
        }

        public static void StoreImage(Guid projectId, Guid imgId)
        {
            if (_image_notTaken.ContainsKey(projectId))
            {
                _image_notTaken[projectId].Add(new ImageForQueue() { Id = imgId });
            }
            
        }

        public static async Task<Image> GetImage(Guid projectId, Guid imgToRelease, long usrId)
        {
            long userId;
            var image = new Image();

            var hadTaken = _image_Taken[projectId];
            var notTaken = _image_notTaken[projectId];

            if (_queues[projectId].TryDequeue(out userId))
            {
                if (imgToRelease.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                {
                    var img = _image_notTaken[projectId].FirstOrDefault();

                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == img.Id);

                    _image_Taken[projectId].Add(img);

                    _image_notTaken[projectId].Remove(img);

                    MonitorHeartbeat(projectId, img);
                }
                else
                {
                    _image_notTaken[projectId].Add(new ImageForQueue() { Id = imgToRelease});

                    var imgToRemoveInDict = _image_Taken[projectId].FirstOrDefault(x => x.Id == imgToRelease);
                    if(imgToRemoveInDict != null)
                        _image_Taken[projectId].Remove(imgToRemoveInDict);

                    var img = _image_notTaken[projectId].FirstOrDefault();

                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == img.Id);

                    img.LastPing = DateTime.Now;

                    _image_Taken[projectId].Add(img);

                    _image_notTaken[projectId].Remove(img);

                    MonitorHeartbeat(projectId, img);
                }

            }

            return image;
        }

        public static async Task<Image> GetImageById(Guid projectId, Guid ImgId, long usrId)
        {
            long userId;
            var image = new Image();

            if (_queues[projectId].TryDequeue(out userId))
            {
                var img = _image_notTaken[projectId].FirstOrDefault(x => x.Id == ImgId);
                if (img != null && _image_notTaken[projectId].Contains(img))
                {
                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == ImgId);

                    img.LastPing = DateTime.Now;

                    _image_Taken[projectId].Add(img);

                    _image_notTaken[projectId].Remove(img);

                    MonitorHeartbeat(projectId, img);

                }
            }

            return image;
        }

        public static bool ReleaseImage(Guid projectId, Guid imgId)
        {
            var img = _image_Taken[projectId].FirstOrDefault(x => x.Id == imgId);
            if (img!= null && _image_Taken[projectId].Contains(img))
            {
                _image_Taken[projectId].Remove(img);
                img.LastPing = new DateTime();
                _image_notTaken[projectId].Add(img);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void MonitorHeartbeat(Guid projectId, ImageForQueue ImageToMonitor)
        {
            Timer mTimer = new Timer();
            mTimer.Elapsed += delegate { CheckHeartbeat(projectId, ImageToMonitor); };
            mTimer.Interval = 60000;
            mTimer.Enabled = true;
            _monitorTimer.Add(ImageToMonitor, mTimer);
        }

        public static void SetTimePing(Guid projectId, Guid imageId, DateTime pingTime)
        {
            var img = _image_Taken[projectId].SingleOrDefault(x => x.Id == imageId);
            if(img != null)
            {
                img.LastPing = pingTime;
            }
        }

        private static void CheckHeartbeat(Guid projectId, ImageForQueue image)
        {
            var timeOut = DateTime.Now;
            var lastPing = image.LastPing;
            var elapsedRunTime = (timeOut - lastPing).TotalSeconds;
            if(elapsedRunTime >= 60)
            {
                ReleaseImage(projectId, image.Id);
                if (_monitorTimer.ContainsKey(image))
                {
                    _monitorTimer[image].Enabled = false;
                    _monitorTimer.Remove(image);
                }
            }
        }
    }
}
