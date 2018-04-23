using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.SignalR
{
    public class Message
    {
        public string Type { get; set; }
        public object Payload { get; set; }
    }
}
