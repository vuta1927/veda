﻿using System;
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
            public string Code { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int TotalTag { get; set; }

        }

        public class ClassForAdd
        {
            public Guid ProjectId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class ClassForUpdate
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
