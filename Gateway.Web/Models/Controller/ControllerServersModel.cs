using System.Collections.Generic;

namespace Gateway.Web.Models.Controller
{
    public class ControllerServersModel
    {
        public string  Name { get; set; }

        public long ControllerId { get; set; }
        public List<ControllerServer> Servers { get; set; }

        public ControllerServersModel(string name)
        {
            Name = name;
        }

        public ControllerServersModel(long controllerId, string controllerName)
        {
            ControllerId = controllerId;
            Name = controllerName;
        }

        public ControllerServersModel()
        {
                
        }
    }
}