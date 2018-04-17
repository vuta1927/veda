using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class ProjectUserModel
    {
        public class ProjectUserForView
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string RoleName { get; set; }
        }

        public class ProjectUserForAdd
        {
            public Guid Id { get; set; }
            public string UserName { get; set; }
            public string RoleName { get; set; }
        }
    }
}
