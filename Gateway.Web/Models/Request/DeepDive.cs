using Gateway.Web.Enums;
using Gateway.Web.Models.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Models.Request
{
    public class DeepDive
    {
        public DeepDive()
        {
            Controllers = Enum.GetNames(typeof(SigmaController))
               .Select(x => new SelectListItem { Value = x, Text = x }).ToList();
            DeepDiveSearch = new DeepDiveSearch() { SearchResource = true };
        }

        public string CorrelationId { get; set; }
        public DeepDiveSearch DeepDiveSearch { get; set; }
        public List<SelectListItem> Controllers { get; set; }
        public List<DeepDiveDto> DeepDiveResults { get; set; }
        public string Controller { get; set; }
    }

    public class DeepDiveSearch
    {
        public string CorrelationId { get; set; }
        public string Search { get; set; }

        public bool SearchPayload { get; set; }
        public bool SearchResource { get; set; }
        public bool SearchError { get; set; }

        public string Controller { get; set; }
    }
}