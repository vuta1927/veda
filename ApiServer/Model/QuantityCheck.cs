﻿using System;
using System.Collections.Generic;
using System.Text;
using VDS.Security;

namespace ApiServer.Model
{
    public class QuantityCheck
    {
        public int Id { get; set; }
        public DateTime QCDate { get; set; }
        public Boolean? Value1 { get; set; }
        public Boolean? Value2 { get; set; }
        public Boolean? Value3 { get; set; }
        public Boolean? Value4 { get; set; }
        public Boolean? Value5 { get; set; }
        public virtual Image Image { get; set; }
        public virtual User UserQc { get; set; }
        public string Comment { get; set; }
    }
}
