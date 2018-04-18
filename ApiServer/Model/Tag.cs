using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ApiServer.Model
{
    public class Tag
    {
        public Tag() => Classes = new JoinCollectionFacade<Class, ClassTag>(ClassTags, ct => ct.Class, t => new ClassTag { Class = t, Tag = this});

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public virtual Image Image { get; set; }
        private ICollection<ClassTag> ClassTags { get; } = new List<ClassTag>();
        [NotMapped]
        public ICollection<Class> Classes { get; set; }

        public QuantityCheck QuantityCheck { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
    }
}
