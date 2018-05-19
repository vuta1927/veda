using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model
{
    public class ProjectSetting
    {
        public int Id { get; set; }
        public int QuantityCheckLevel { get; set; }
        public double TaggTimeValue { get; set; }
        public virtual Project Project { get; set; }
    }
}
