using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Controllers
{
    public class UsageModel
    {
        public UsageModel()
        {
            Rows = new List<string>();
            HistoryStartTime = DateTime.Today.AddDays(-7);
        }

        public List<string> Rows { get; private set; }

        public DateTime HistoryStartTime { get; set; }
    }
}