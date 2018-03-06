using System.Collections.Generic;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.User
{
    public class HistoryModel : BaseControllerModel, IUserModel
    {
        public HistoryModel(long id, string name, string domain) : base(name)
        {
            Id = id;
            Domain = domain;
            Requests = new List<HistoryItem>();
        }
        public long Id { get; set; }

        public string Domain { get; set; }

        public List<HistoryItem> Requests { get; private set; }

        public string Login
        {
            get { return Name; }
        }
    }
}