using System.Collections.Generic;
using System.Web.Mvc;

namespace Gateway.Web.Models.Controller
{
    [Bind(Include = "Versions")]
    public class VersionsModel : BaseControllerModel
    {
        public VersionsModel(string name) : base(name)
        {
            Versions = new List<Version>();
        }

        public string[] UpdateResults { get; set; }
        public List<Version> Versions { get; private set; }

    }
}