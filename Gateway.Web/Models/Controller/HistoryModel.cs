using Gateway.Web.Models.Controllers;
using System.Collections.Generic;

namespace Gateway.Web.Models.Controller
{
    public class HistoryModel : BaseControllerModel
    {
        public HistoryModel(string name) : base(name)
        {
            Requests = new List<HistoryItem>();
            ControllerDetail = new ControllerDetail();
        }

        public List<HistoryItem> Requests { get; private set; }

        public string SearchText { get; set; }
        public ControllerDetail ControllerDetail { get; set; }
    }
}