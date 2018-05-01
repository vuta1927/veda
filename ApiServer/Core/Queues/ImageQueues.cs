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
        private static Dictionary<Guid, ConcurrentQueue<long>> _queues;

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

                _queues = new Dictionary<Guid, ConcurrentQueue<long>>();

                _queues.Add(projectId, queue);

                _queues[projectId].Enqueue(userId);

                var ids = _context.Images.Where(x => x.Project.Id == projectId).Select(x => x.Id).ToList();

                imageNotTaken.Add(projectId, ids);
            }
        }

        public static async Task<Image> GetImage(Guid projectId, long usrId)
        {
            long userId;
            var imageId = imageNotTaken[projectId].FirstOrDefault();
            var image = new Image();

            if (_queues[projectId].TryDequeue(out userId))
            {
                if (imageId != null)
                {
                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == imageId);

                    imageHadTaken[projectId].Append(imageId);

                    imageNotTaken[projectId].Remove(imageId);
                }
                else
                {
                    imageNotTaken[projectId] = imageHadTaken[projectId];

                    imageHadTaken[projectId] = new List<Guid>();

                    image = await _context.Images.Include(x => x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == imageId);

                    imageHadTaken[projectId].Append(imageId);

                    imageNotTaken[projectId].Remove(imageId);
                }

            }

            return image;
        }
    }
}
