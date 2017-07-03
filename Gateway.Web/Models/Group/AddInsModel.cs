using System.Collections.Generic;
using Gateway.Web.Models.AddIn;

namespace Gateway.Web.Models.Group
{
    public class AddInsModel
    {
        public AddInsModel(long id)
        {
            Id = id;
            Items = new List<AddInModel>();
        }

        public long Id { get; set; }
        public List<AddInModel> Items { get; private set; }
    }
}