using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.User
{
    [XmlType("UserSite")]
    public class SitesModel : IUserModel
    {
        public SitesModel(long id)
        {
            Id = id;
        }

        public SitesModel() { }

        public long Id { get; set; }

        public string Login { get; set; }

        public string Domain { get; set; }

        public List<SiteModel> Sites { get; set; }

        public List<SiteModel> InheritedSites { get; set; }

        public List<SelectListItem> AvailableSites { get; set; }
    }
}