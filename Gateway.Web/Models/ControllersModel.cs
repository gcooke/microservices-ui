using System;
using System.Collections.Generic;

namespace Gateway.Web.Models
{
    public class ControllersModel
    {
        public ControllersModel()
        {
            Controllers = new List<ControllerModel>();
            HistoryStartTime = DateTime.Today.AddDays(-7);
        }

        public List<ControllerModel> Controllers { get; private set; }

        public DateTime HistoryStartTime { get; set; }
    }
}