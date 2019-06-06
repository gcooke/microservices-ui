using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.ServerResource
{
    public class ServerControllerModel
    {
        public int ServerId { get; set; }

        public string ServerName { get; set; }

        public List<ServerController> Controllers { get; set; }
    }
}