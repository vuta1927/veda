using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ApiServer.Model
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClassColor { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public virtual Project Project { get; set; }
    }
}
