using System.Collections.Generic;

namespace Gateway.Web.Models
{
    public class Catalogue
    {
        public Catalogue()
        {
            Controllers = new List<ControllerModel>();
        }

        public List<ControllerModel> Controllers { get; private set; }
    }
}