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
        public virtual Project Project { get; set; }
        public virtual Role Role { get; set; }
    }
}
