﻿using System;
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
        public DateTime TaggedDate { get; set; }
        public string Classes { get; set; }
        public bool QcStatus { get; set; }
        public virtual User UserQc { get; set; }
        public DateTime QcDate { get; set; }
        public virtual User UserTagged { get; set; }
        public virtual Project Project { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public virtual QuantityCheck QuantityCheck { get; set; }
        public double TagTime { get; set; }
    }
}
