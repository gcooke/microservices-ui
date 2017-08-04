using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.User
{
    [XmlType("UserPortfolio")]
    public class PortfoliosModel : IUserModel
    {
        public PortfoliosModel(long id)
        {
            Id = id;
        }

        public PortfoliosModel() { }

        public long Id { get; set; }

        public List<PortfolioModel> Portfolios { get; set; }

        public List<PortfolioModel> InheritedPortfolios { get; set; }

        public List<SelectListItem> AvailablePortfolios { get; set; }

        public string Login { get; set; }
    }
}