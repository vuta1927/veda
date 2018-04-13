using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public class UserView
    {
        public long id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int accessFailedCount { get; set; }
        public bool isLockoutEnabled { get; set; }
        public DateTime? lockoutEndDateUtc { get; set; }
        public bool isActive { get; set; }
    }
}
