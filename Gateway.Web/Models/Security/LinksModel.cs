using System.Collections.Generic;
using System.Web.Mvc;

namespace Gateway.Web.Models.Security
{
    public class LinksModel
    {
        public LinksModel()
        {
            Items = new List<LinkModel>();
            AvailableTypes = new List<SelectListItem>();
        }

        public List<LinkModel> Items { get; private set; }
        public List<SelectListItem> AvailableTypes { get; private set; }

        public void PopulateSelectionLists()
        {
            AvailableTypes.Add(new SelectListItem() { Value = "Shortcut", Text = "Shortcut" });
            AvailableTypes.Add(new SelectListItem() { Value = "UserWebsite", Text = "User Website" });
            AvailableTypes.Add(new SelectListItem() { Value = "SupportWebsite", Text = "Support Website" });            
        }
    }
}