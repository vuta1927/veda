using System;
using VDS.Domain.Entities;

namespace VDS.Settings
{
    public class Setting : Entity<Guid>
    {
//        [Required, ShortString]
        public string Category { get; set; }
//        [Required, ShortString]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}