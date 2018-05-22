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
            public List<UserProject> UserProjects { get; set; }
        }

        public class Project
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class UserProject
        {
            public long UserId { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string[] RoleNames { get; set; }
            public double TaggedTime { get; set; }
            public int TotalTags { get; set; }
            public int TotalQcs { get; set; }
        }
    }
}
