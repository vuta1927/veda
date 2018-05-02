using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.Security;

namespace ApiServer.Model
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public long UserId { get; set; }
        public virtual Project Project { get; set; }
        public Guid ProjectId { get; set; }
        public virtual Role Role { get; set; }
        public int RoleId { get; set; }
    }
}
