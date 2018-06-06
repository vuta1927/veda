using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Model.views
{
    public static class ClassModel
    {
        public class ClassForView
        {
            public int Id { get; set; }
            public string ClassColor { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int TotalTag { get; set; }
            public string ImportDisplay { get; set; }

            public string Project { get; set; }
        }
        public class ClassForMerge
        {
            public int Id { get; set; }
            public string ClassColor { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }

            public string Project { get; set; }
        }

        public class ClassForAdd
        {
            public Guid ProjectId { get; set; }
            public string ClassColor { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class ClassForUpdate
        {
            public int Id { get; set; }
            public string ClassColor { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
