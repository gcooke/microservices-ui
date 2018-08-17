using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Models.ServerResource
{
    public class ResourceConfigModel
    {
        public ResourceConfigModel()
        {
            Configs = new List<ItemResourceConfig>();
            AllResources = new List<ExternalResourceConfig>();
        }

        public List<ItemResourceConfig> Configs { get; set; }

        public ResourceConfigType ConfigType {get;set;}

        public List<ExternalResourceConfig> AllResources { get; set; }
    }
}