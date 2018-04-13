using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class ProjectModel
    {
        public class ProjectForView 
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int TotalImg { get; set; }
            public int TotalImgNotTagged { get; set; }
            public int TotalImgNotClassed { get; set; }
            public int TotalImgQC { get; set; }
            public int TotalImgNotQC { get; set; }
            public bool IsDisabled { get; set; }
            public string Description { get; set; }
            public string Note { get; set; }
            public string Usernames { get; set; }
        }

        public class ProjectForCreate {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Note { get; set; }
            
        }

        public class ProjectForUpdate
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Note { get; set; }

        }
    }
}
