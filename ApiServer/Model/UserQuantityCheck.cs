using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.Security;

namespace ApiServer.Model
{
    public class UserQuantityCheck
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public long UserId { get; set; }
        public virtual QuantityCheck QuantityCheck { get; set; }
        public int QuantityCheckId { get; set; }
    }
}
