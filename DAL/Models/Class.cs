using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Models
{
    public class Class
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
