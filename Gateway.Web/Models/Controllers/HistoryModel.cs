using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controller;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Controllers
{
    public class HistoryModel
    {
        public HistoryModel()
        {
            Requests = new List<HistoryItem>();
        }

        public List<HistoryItem> Requests { get; private set; }
    }
}