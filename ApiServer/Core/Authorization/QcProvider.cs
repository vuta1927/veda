using System;
using System.Collections.Generic;
using System.Text;
using ApiServer.Model;
namespace ApiServer.Core.Authorization
{
    class QcProvider
    {
        public static readonly QuantityCheckType Checked = new QuantityCheckType { Name = "Passed" };
        public static readonly QuantityCheckType UnCheck = new QuantityCheckType { Name = "False" };

        public IEnumerable<QuantityCheckType> GetQcs() {
            return new List<QuantityCheckType>
            {
                Checked,
                UnCheck
            };
        }
    }
}
