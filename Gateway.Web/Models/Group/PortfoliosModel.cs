using System.Collections.Generic;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Group
{
    public class PortfoliosModel : IGroupModel
    {
        public PortfoliosModel(long id)
        {
            Id = id;
            Items = new List<PortfolioModel>();
        }

        public long Id { get; set; }
        public List<PortfolioModel> Items { get; private set; }
        public string Name { get; set; }
        public string Domain { get; set; }
    }
}