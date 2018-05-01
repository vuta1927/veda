using ApiServer.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.Queues
{
    public static class ImageQueues
    {
        private static Dictionary<Guid, ConcurrentQueue<long>> _queues = new Dictionary<Guid, ConcurrentQueue<long>>();

        private static VdsContext _context;

        private static Dictionary<Guid, List<Guid>> imageNotTaken = new Dictionary<Guid, List<Guid>>();

        private static Dictionary<Guid, List<Guid>> imageHadTaken = new Dictionary<Guid, List<Guid>>();

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

                imageNotTaken.Add(projectId, ids);
                imageHadTaken.Add(projectId, new List<Guid>());
            }
        }

        public static async Task<Image> GetImage(Guid projectId, Guid imgToRelease, long usrId)
        {
            long userId;
            var image = new Image();

            var hadTaken = imageHadTaken[projectId];
            var notTaken = imageNotTaken[projectId];

            if (_queues[projectId].TryDequeue(out userId))
            {
                if (imgToRelease.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                {
                    var imageId = imageNotTaken[projectId].FirstOrDefault();

                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == imageId);

                    imageHadTaken[projectId].Add(imageId);

                    imageNotTaken[projectId].Remove(imageId);
                }
                else
                {
                    imageNotTaken[projectId].Add(imgToRelease);

                    imageHadTaken[projectId].Remove(imgToRelease);

                    var imageId = imageNotTaken[projectId].FirstOrDefault();

                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == imageId);

                    imageHadTaken[projectId].Add(imageId);

                    imageNotTaken[projectId].Remove(imageId);
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
                if (imageNotTaken[projectId].Contains(ImgId))
                {
                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == ImgId);

                    imageHadTaken[projectId].Add(ImgId);

                    imageNotTaken[projectId].Remove(ImgId);
                }
            }

            return image;
        }

        public static bool ReleaseImage(Guid projectId, Guid imgId)
        {
            if (imageHadTaken[projectId].Contains(imgId))
            {
                imageHadTaken[projectId].Remove(imgId);
                imageNotTaken[projectId].Add(imgId);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
