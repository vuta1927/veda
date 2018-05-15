using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ApiServer.Core.SignalR;
using ApiServer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VDS.AspNetCore.Mvc.Authorization;

namespace ApiServer.Hubs
{
    public class VdsHub : Hub
    {
        //public override async Task OnConnectedAsync()
        //{
        //    await Clients.All.SendAsync("SendAction", Context.User.Identity.Name, "joined");
        //}

        //public override async Task OnDisconnectedAsync(Exception ex)
        //{
        //    await Clients.All.SendAsync("SendAction", Context.User.Identity.Name, "left");
        //}

        public async Task Send(string message)
        {
            await Clients.All.SendAsync("SendMessage", Context.User.Identity.Name, message);
        }

        public string InvokeHubMethod()
        {
            return "ConnectionID";  //ConnectionID will the Id as string that you want outside the hub
        }

    }
}
