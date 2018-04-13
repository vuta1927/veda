using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class PermissionModel
    {
        public class PermissionBase
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string DisplayName { get; set; }
            public string Descriptions { get; set; }
        }
        public class PermissionForView : PermissionBase
        {

        }

        public class PermissionWithCategor: PermissionBase
        {
            public bool IsCheck { get; set; }
        }
    }
}
