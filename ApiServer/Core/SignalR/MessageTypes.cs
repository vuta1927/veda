using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.SignalR
{
    public static class MessageTypes
    {
        public const string UserUsing = "userUsing";
        public const string UserRelease = "userRelease";
        public const string ReloadImageData = "reloadImageData";
    }
}
