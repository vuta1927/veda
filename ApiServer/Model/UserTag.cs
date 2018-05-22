using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.Security;

namespace ApiServer.Model
{
    public class UserTag
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public long UserId { get; set; }
        public virtual Tag Tag { get; set; }
        public int TagId { get; set; }
        public Guid ImageId { get; set; }
    }
}
