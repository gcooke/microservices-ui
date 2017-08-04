using System.Collections.Generic;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Security
{
    public class AddInsModel
    {
        public AddInsModel()
        {
            Items = new List<AddInModel>();
        }

        public List<AddInModel> Items { get; private set; }
    }
}