using Gateway.Web.Database;
using System.Collections.Generic;

namespace Gateway.Web.Models.ServerResource
{
    public class ServerResourceModel
    {
       public IList<Server> Servers { get; set; }
    }
}