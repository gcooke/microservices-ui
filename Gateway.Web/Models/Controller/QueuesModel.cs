using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Controller
{
    public class QueuesModel : BaseControllerModel
    {
        public QueuesModel(string name) : base(name)
        {
            Current = new List<QueueModel>();
        }

        public IEnumerable<string> Versions { get; set; } 

        public IList<QueueModel> Current { get; set; }
    }
}