using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class ImageModel
    {
        public class ImageForView
        {
            public Guid Id { get; set; }
            public string Path { get; set; }
            public bool Ignored { get; set; }
            public int TotalClass { get; set; }
            public int TagHasClass { get; set; }
            public int TagNotHasClass { get; set; }
            public DateTime? TaggedDate { get; set; }
            public string Classes { get; set; }
            public string QcStatus { get; set; }
            public string UserQc { get; set; }
            public DateTime? QcDate { get; set; }
            public string UserTagged { get; set; }
            public double TagTime { get; set; }
            public string UserUsing { get; set; }
        }

        public class ImageForView2 : ImageForView
        {
            public new List<QuantityCheckModel.Qc> QcStatus { get; set; }
        }
    }
}
