using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using VDS.Security;

namespace ApiServer.Model
{
    public class Tag
    {
        public Tag() => Classes = new JoinCollectionFacade<Class, ClassTag>(ClassTags, ct => ct.Class, c => new ClassTag { Class = c, Tag = this });
        public int Id { get; set; }
        public int Index { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double height { get; set; }
        public virtual Image Image { get; set; }
        public QuantityCheck QuantityCheck { get; set; }
        public virtual User UserTagged { get; set; }
        private ICollection<ClassTag> ClassTags { get; } = new List<ClassTag>();
        [NotMapped]
        public ICollection<Class> Classes { get; set; }
    }
}
