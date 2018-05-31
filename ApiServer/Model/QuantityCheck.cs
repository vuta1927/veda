using System;
using System.Collections.Generic;
using System.Text;
using VDS.Security;

namespace ApiServer.Model
{
    public class QuantityCheck
    {
        public int Id { get; set; }
        public DateTime QCDate { get; set; }
        public Boolean? Value1 { get; set; }
        public Boolean? Value2 { get; set; }
        public Boolean? Value3 { get; set; }
        public Boolean? Value4 { get; set; }
        public Boolean? Value5 { get; set; }
        public string CommentLevel1 { get; set; }
        public string CommentLevel2 { get; set; }
        public string CommentLevel3 { get; set; }
        public string CommentLevel4 { get; set; }
        public string CommentLevel5 { get; set; }
        public Guid ImageId { get; set; }
        public Image Image { get; set; }
        public virtual List<UserQuantityCheck> UsersQc { get; set; }
        public string Comment { get; set; }
    }
}
