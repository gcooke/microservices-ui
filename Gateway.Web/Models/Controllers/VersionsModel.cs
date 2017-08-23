using System.Collections.Generic;

namespace Gateway.Web.Models.Controllers
{
    public class VersionsModel
    {
        public VersionsModel()
        {
            Log = new List<string>();
            Loading = false;
            Loaded = false;
        }

        public bool Loading { get; set; }
        public bool Loaded { get; set; }
        public bool Success { get; set; }
        public List<string> Log { get; set; }
    }
}