using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class ProjectSettingModel
    {
        public class ProjectSetting
        {
            public int Id { get; set; }
            public int QuantityCheckLevel { get; set; }
            public double TaggTimeValue { get; set; }
        }
        public class ProjectSettingForAdd
        {
            public int QuantityCheckLevel { get; set; }
            public double TaggTimeValue { get; set; }
            public Guid ProjectId { get; set; }
        }

        public class ProjectSettingForUpdate
        {
            public int Id { get; set; }
            public int QuantityCheckLevel { get; set; }
            public double TaggTimeValue { get; set; }
            public Guid ProjectId { get; set; }
        }
    }
}
