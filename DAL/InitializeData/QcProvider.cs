﻿using System;
using System.Collections.Generic;
using System.Text;
using DAL.Models;
namespace DAL.InitializeData
{
    class QcProvider
    {
        public static readonly QuantityCheckType Checked = new QuantityCheckType { Name = "Passed", Value = 100 };
        public static readonly QuantityCheckType UnCheck = new QuantityCheckType { Name = "False", Value = 0 };

        public IEnumerable<QuantityCheckType> GetQcs() {
            return new List<QuantityCheckType>
            {
                Checked,
                UnCheck
            };
        }
    }
}
