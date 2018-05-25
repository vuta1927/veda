using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.Email
{
    public interface IEmailHelper
    {
        Task Send(string destination, string subject, string body);
        Task Broadcast(string subject, string body);
    }
}
