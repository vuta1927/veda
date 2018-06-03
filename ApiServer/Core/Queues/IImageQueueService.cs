using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiServer.Model;

namespace ApiServer.Core.Queues
{
    public interface IImageQueueService
    {
        Image GetImage(Guid projectId, Guid imgToRelease, long usrId);
        Image GetImageById(Guid projectId, Guid ImgId, long usrId);
        void AddImage(Guid projectId, Guid imageId);
        Task DeleteImage(Guid projectId, Guid imageId);
        Task<bool> ReleaseImage(Guid projectId, Guid imgId);
        void SetTimePing(Guid projectId, Guid imageId, DateTime pingTime);
        Task<string> GetUserUsing(Guid projectId, Guid imageId);
    }
}
