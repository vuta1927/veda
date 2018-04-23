using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.SignalR
{
    public interface ITypedHubClient
    {
        Task Send(string type, object payload);
    }
}
