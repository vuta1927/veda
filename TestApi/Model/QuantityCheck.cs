using System;
using System.Collections.Generic;
using System.Text;
using VDS.Security;

namespace ApiServer.Model
{
    public class QuantityCheck
    {
        public int Id { get; set; }
        public DateTime QCDate { get; set; }
        public virtual QuantityCheckType QuantityCheckType { get; set; }
        public virtual Tag Tag { get; set; }
        public virtual User UserQc { get; set; }
    }
}
