using System.Collections.Generic;

namespace Gateway.Web.Models.Group
{
    public class SitesModel
    {
        public SitesModel(long id)
        {
            Id = id;
            Items = new List<SiteModel>();
        }

        public long Id { get; set; }
        public List<SiteModel> Items { get; private set; }
    }
}