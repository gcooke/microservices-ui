using System.Collections.Generic;
using Gateway.Web.Models.Controller;

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