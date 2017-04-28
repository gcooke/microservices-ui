using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Services;

namespace Gateway.Web.Models.Controller
{
    public class WorkersModel : BaseControllerModel
    {
        public WorkersModel(string name) : base(name)
        {
            State = new List<ControllerInformation>();
            Processes = new List<ProcessInformation>();
        }

        public List<ControllerInformation> State { get; private set; }

        public List<ProcessInformation> Processes { get; private set; }
    }
}