using System.Collections.Generic;
using Gateway.Web.Database;

namespace Gateway.Web.Models.Controllers
{
    public class AliasesModel
    {
        public AliasesModel()
        {
            Items = new List<Alias>();
        }

        public string[] UpdateResults { get; set; }
        public List<Alias> Items { get; private set; }
    }
}