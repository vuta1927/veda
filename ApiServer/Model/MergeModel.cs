using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiServer.Model.views;

namespace ApiServer.Model
{
    public static class MergeModel
    {
        public class Merge
        {
            public string ProjectName { get; set; }
            public List<Guid> Projects { get; set; }
            public string ConnectionId { get; set; }
            public List<ClassModel.ClassForMerge> Classes { get; set; }
            public List<MergeClass> MergeClasses { get; set; }
            public List<MergeProjectUser> Users { get; set; }
            public FilterOptions FilterOptions { get; set; }
        }

        public class MergeProjectUser
        {
            public string UserName { get; set; }
            public string RoleName { get; set; }
        }

        public class MergeClass
        {
            public List<ClassModel.ClassForMerge> OldClasses { get; set; }
            public ClassModel.ClassForMerge NewClass { get; set; }
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
    }
}
