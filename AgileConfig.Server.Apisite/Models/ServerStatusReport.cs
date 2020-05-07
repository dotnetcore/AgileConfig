using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Models
{
    public class ServerStatusReport
    {
        public ClientInfos WebsocketCollectionReport { get; set; }

        public int AppCount { get; set; }

        public int ConfigCount { get; set; }

        public int NodeCount { get; set; }
    }
}
