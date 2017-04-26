using System.Collections.Generic;
using Gateway.Web.Database;

namespace Gateway.Web.Models.Controller
{
    public class HistoryModel : BaseControllerModel
    {
        public HistoryModel(string name) : base(name)
        {
            Requests = new List<HistoryItem>();
        }

        public List<HistoryItem> Requests { get; private set; }
    }
}