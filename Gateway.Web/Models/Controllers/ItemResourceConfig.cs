using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controllers
{
    public class ItemResourceConfig
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public IList<ExternalResourceConfig> AllowableResources { get; set; }
    }
}