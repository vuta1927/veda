using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class TagModel
    {
        public class TagForView
        {
            public int Id { get; set; }
            public Guid ImageId { get; set; }
            public IEnumerable<int> ClassId { get; set; }
            public double Top { get; set; }
            public double Left { get; set; }
            public int QuantityCheckId { get; set; }
        }

        public class TagForAddOrUpdate
        {
            public int Id { get; set; }
            public Guid ImageId { get; set; }
            public IEnumerable<int> ClassIds { get; set; }
            public double Top { get; set; }
            public double Left { get; set; }
        }

        public class TagForUpdate
        {
            public int Id { get; set; }
            public Guid ImageId { get; set; }
            public IEnumerable<int> ClassIds { get; set; }
            public double Top { get; set; }
            public double Left { get; set; }
        }
    }
}
