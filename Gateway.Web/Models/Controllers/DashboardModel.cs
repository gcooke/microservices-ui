using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Controllers
{
    public class DashboardModel
    {
        public DashboardModel()
        {
            Controllers = new List<ControllerStats>();
            HistoryStartTime = DateTime.Today.AddDays(-7);
        }

        public List<ControllerStats> Controllers { get; private set; }

        public DateTime HistoryStartTime { get; set; }
    }
}