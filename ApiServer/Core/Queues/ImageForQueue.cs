﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.Queues
{
    public class ImageForQueue
    {
        public Guid Id { get; set; }
        public DateTime LastPing { get; set; }
    }
}