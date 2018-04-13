using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VDS.Security;

namespace DAL.Models
{
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Path { get; set; }
        public bool Ignored { get; set; }
        public int TotalClass { get; set; }
        public int TagHasClass { get; set; }
        public int TagNotHasClass { get; set; }
        public DateTime TaggedDate { get; set; }
        public string Classes { get; set; }
        public string QcStatus { get; set; }
        public virtual User UserQc { get; set; }
        public DateTime QcDate { get; set; }
        public virtual User UserTagged { get; set; }
        public virtual Project Project { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
