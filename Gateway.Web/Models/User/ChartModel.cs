using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.User
{
    public class ChartModel : BaseControllerModel, IUserModel
    {
        public ChartModel(long id, string name, string domain, string fullName) : base(name)
        {
            Id = id;
            Domain = domain;
            FullName = fullName;
        }

        public long Id { get; set; }

        public string Domain { get; set; }

        public string Login => Name;

        public string FullName { get; set; }

        public RequestsChartModel Chart { get; set; }
    }
}