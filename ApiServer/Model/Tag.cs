using System;
using System.Collections.Generic;
using System.Text;

namespace ApiServer.Model
{
    public class Tag
    {
        public int Id { get; set; }
        public virtual Image Image { get; set; }
        public virtual Class Class { get; set; }
        public QuantityCheck QuantityChecks { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
    }
}
