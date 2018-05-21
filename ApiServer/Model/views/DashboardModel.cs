using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class DashboardModel
    {
        public class Dashboard
        {
            public ProjectAnalist Project { get; set; }
        }

        public class ProjectAnalist
        {
            public int TotalImages { get; set; }
            public int ImagesTagged { get; set; }
            public int ImagesHadQc { get; set; }
            public string TotalTaggedTime { get; set; }
            public int TotalTags { get; set; }
            public int TotalTagsHaveClass { get; set; }
        }

        public class Project
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }
    }
}
