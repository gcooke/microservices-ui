using System.Collections.Generic;
using System.Web.Mvc;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Security
{
    public class ApplicationsModel
    {
        public ApplicationsModel()
        {
            Items = new List<ManifestModel>();
            From = new List<SelectListItem>();
            To = new List<SelectListItem>();

            Types = new List<SelectListItem>();
            Types.Add(new SelectListItem() { Value = "AddIn", Text = "Add-In" });
            Types.Add(new SelectListItem() { Value = "Application", Text = "Application" });
        }

        public List<ManifestModel> Items { get; private set; }
        public ICube ActiveItems { get; set; }
        public List<SelectListItem> Types { get; private set; }
        public List<SelectListItem> From { get; set; }
        public List<SelectListItem> To { get; set; }
    }
}