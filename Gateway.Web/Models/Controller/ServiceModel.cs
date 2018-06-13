using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Controller
{
    public class ServiceModel : BaseControllerModel
    {
        public ServiceModel(string name) : base(name)
        {
        }

        public List<ServiceInfoModel> Services { get; set; }
    }
}