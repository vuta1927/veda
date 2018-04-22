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
            public int Index { get; set; }
            public Guid ImageId { get; set; }
            public IEnumerable<int> ClassIds { get; set; }
            public double Top { get; set; }
            public double Left { get; set; }
            public double Width { get; set; }
            public double height { get; set; }
            public int QuantityCheckId { get; set; }
        }

        public class TagForAddOrUpdate
        {
            public int Id { get; set; }
            public int Index { get; set; }
            public Guid ImageId { get; set; }
            public IEnumerable<int> ClassIds { get; set; }
            public double Top { get; set; }
            public double Left { get; set; }
            public double Width { get; set; }
            public double height { get; set; }
        }

        public class TagForUpdate
        {
            public int Id { get; set; }
            public int Index { get; set; }
            public Guid ImageId { get; set; }
            public IEnumerable<int> ClassIds { get; set; }
            public double Top { get; set; }
            public double Left { get; set; }
            public double Width { get; set; }
            public double height { get; set; }
        }

        public class DataUpdate{
            public long UserId { get; set; }
            public IEnumerable<TagForUpdate> Tags { get; set; }
            public IEnumerable<ExcluseArea> ExcluseAreas { get; set; }
        }

        public class ExcluseArea
        {
            public Coordinate[] Paths { get; set; }
        }

        public class Coordinate
        {
            public float X { get; set; }
            public float Y { get; set; }
        }
    }
}
