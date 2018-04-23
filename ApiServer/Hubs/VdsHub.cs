using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ApiServer.Core.SignalR;
namespace ApiServer.Hubs
{
    public class VdsHub: Hub
    {
        public Task Send(string message)
        {
            return Clients.Caller.SendAsync("Send", message);
        }
    }
}
