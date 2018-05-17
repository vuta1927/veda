using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model
{
    public static class ExportModel
    {
        public class Exoprt
        {
            public string[] Classes { get; set; }
            public FilterOptions FilterOptions { get; set; }
        }

        public class FilterOptions
        {
            public List<QcOption> QcOptions { get; set; }
        }

        public class QcOption
        {
            public int Index { get; set; }
            public bool Value { get; set; }
        }

        public class Tag
        {
            public int ClassId { get; set; }
            public double CenterX { get; set; }
            public double CenterY { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public Image Image { get; set; }
        }
    }
}
