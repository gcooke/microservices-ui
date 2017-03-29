using System.Collections.Generic;
using Gateway.Web.Database;

namespace Gateway.Web.Models
{
    public class ControllerDetailModel
    {
        public ControllerDetailModel()
        {
            Requests = new List<Request>();
            Versions = new List<Version>();
        }

        public string Name { get; set; }

        public int RecentRequestsCount { get { return 20; } }

        public List<Version> Versions { get; private set; }

        public List<Request> Requests { get; private set; }
    }
    
}