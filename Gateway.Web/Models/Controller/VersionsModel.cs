using System.Collections.Generic;

namespace Gateway.Web.Models.Controller
{
    public class VersionsModel : BaseControllerModel
    {
        public VersionsModel(string name) : base(name)
        {
            Versions = new List<Version>();
        }

        public List<Version> Versions { get; private set; }
    }
}