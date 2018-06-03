using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ApiServer.Core.Queues
{
    public static class RuntimeDataContainer
    {
        public static Dictionary<Guid, ConcurrentQueue<Guid>> ImageStorage = new Dictionary<Guid, ConcurrentQueue<Guid>>();

        public static Dictionary<Guid, List<Guid>> ImageStorageOptional = new Dictionary<Guid, List<Guid>>(); // GetImageById will use this instead of ImageStorage

        public static Dictionary<Guid, List<ImageForQueue>> ImagesUsed = new Dictionary<Guid, List<ImageForQueue>>();

        public static Dictionary<ImageForQueue, Timer> MonitorTimers = new Dictionary<ImageForQueue, Timer>();
    }
}
