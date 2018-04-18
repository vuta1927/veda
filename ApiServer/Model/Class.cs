using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ApiServer.Model
{
    public class Class
    {
        public Class() => Tags = new JoinCollectionFacade<Tag, ClassTag>(ClassTags, ct => ct.Tag, t => new ClassTag { Class = this, Tag = t });
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        private ICollection<ClassTag> ClassTags { get; } = new List<ClassTag>();
        [NotMapped]
        public ICollection<Tag> Tags { get; set; }
        public virtual Project Project { get; set; }
    }
}
