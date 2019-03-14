using System.Collections.Generic;
using System.Web.Mvc;

namespace Gateway.Web.Models.Controller
{
    public class ServiceModel : BaseControllerModel
    {
        public ServiceModel(string name) : base(name)
        {
        }

        public List<ServiceInfoModel> Services { get; set; }

        public IList<SelectListItem> Versions { get; set; }
    }
}