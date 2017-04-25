using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Controllers
{
    public class ServersModel
    {
        public ServersModel()
        {
            Servers = new List<Server>();
        }

        public List<Server> Servers { get; private set; }
    }

    public class Server
    {
        public string Name { get; set; }
        public int Queues { get; set; }
        public int Workers { get; set; }
    }
}