using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VDS.Security;

namespace ApiServer.Model
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public ICollection<ProjectUser> Users { get; set; } = new List<ProjectUser>();
    }
}
