using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class QuantityCheckModel
    {
        public class QcObjectForAdd
        {
            public long UserId { get; set; }
            public Guid ImageId { get; set; }
            public bool QcValue { get; set; }
            public string QcComment { get; set; }
        }
    }
}
