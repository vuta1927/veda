﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Models
{
    public class QuantityCheckType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public ICollection<QuantityCheck> quantityChecks { get; set; }
    }
}
