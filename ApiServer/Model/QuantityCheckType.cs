using System;
using System.Collections.Generic;
using System.Text;

namespace ApiServer.Model
{
    public class QuantityCheckType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<QuantityCheck> quantityChecks { get; set; }
    }
}
