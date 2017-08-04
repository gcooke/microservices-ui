using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Group
{
    public class SitesModel
    {
        public SitesModel(long id)
        {
            Id = id;
            AvailableSites = new List<SelectListItem>();
            Items = new List<SiteModel>();
        }

        public long Id { get; private set; }
        public List<SiteModel> Items { get; private set; }
        public List<SelectListItem> AvailableSites { get; private set; }
    }
}