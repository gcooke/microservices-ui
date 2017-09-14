using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Security
{
    public class AddInsModel
    {
        public AddInsModel()
        {
            Items = new List<AddInModel>();
            From = new List<SelectListItem>();
            To = new List<SelectListItem>();
        }

        public List<AddInModel> Items { get; private set; }
        public ReportTable ActiveVersions { get; set; }
        public List<SelectListItem> From { get; set; }
        public List<SelectListItem> To { get; set; }
    }
}