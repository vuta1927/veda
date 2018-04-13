using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.Security.Permissions;

namespace ApiServer.Model.views
{
    public static class RoleModel
    {
        public class RoleBase
        {
            public int Id { get; set; }
            public string RoleName { get; set; }
            public string Descriptions { get; set; }
        }
        public class RoleForUpdate : RoleBase
        {
            public long LastModifierUserId { get; set; }
        }

        public class RoleForCreate : RoleBase
        {
            public long CreatorUserId { get; set; }
        }
        
        public class RoleForView : RoleBase
        {
            public List<PermissionModel.PermissionWithCategor> Permissions { get; set; }
        }
    }
}
