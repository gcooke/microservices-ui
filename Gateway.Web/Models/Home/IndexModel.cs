using System;
using System.Collections.Generic;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.Home
{
    public class IndexModel
    {
        public IndexModel()
        {
            Batches = new List<RiskBatchGroup>();
            Services = new List<ServiceState>();
            Databases = new List<DatabaseState>();
            Controllers = new List<ControllerState>();
        }

        public List<RiskBatchGroup> Batches { get; private set; }

        public List<ServiceState> Services { get; private set; }

        public List<DatabaseState> Databases { get; private set; }

        public List<ControllerState> Controllers { get; private set; }
    }
}