using System.Collections.Generic;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.User
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