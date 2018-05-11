using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.Queues
{
    public class UserImageForHub
    {
        public string UserName { get; set; }
        public Guid ImageId { get; set; }
    }
}
