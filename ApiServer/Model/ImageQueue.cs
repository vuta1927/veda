using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model
{
    public class ImageQueue
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ImageId { get; set; }
        public long UserId { get; set; }
    }
}
