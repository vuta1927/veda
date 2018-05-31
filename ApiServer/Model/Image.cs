using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VDS.Security;

namespace ApiServer.Model
{
    public class Image
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public bool Ignored { get; set; }
        public int TotalClass { get; set; }
        public int TagHasClass { get; set; }
        public int TagNotHasClass { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public DateTime? TaggedDate { get; set; }
        public string Classes { get; set; }
        public string QcStatus { get; set; }
        public virtual List<User> UsersQc { get; set; }
        public DateTime? QcDate { get; set; }
        public virtual List<User> UsersTagged { get; set; }
        public virtual Project Project { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public QuantityCheck QuantityCheck { get; set; }
        public double TagTime { get; set; }
        public virtual List<UserTaggedTime> UserTaggedTimes { get; set; }
    }
}
