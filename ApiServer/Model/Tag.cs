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
        public int Id { get; set; }
        public int Index { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double WidthPixel { get; set; }
        public double HeightPixel { get; set; }
        public virtual Image Image { get; set; }
        public DateTime TaggedDate { get; set; }
        public QuantityCheck QuantityCheck { get; set; }
        public virtual List<UserTag> UsersTagged { get; set; }
        public int? ClassId { get; set; }
        public virtual Class Class { get; set; }
    }
}
