using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gateway.Web.Models.Security
{
    public class BusinessFunctionsModel
    {
        public IList<BusinessFunction> BusinessFunctions { get; set; }
        public IList<SelectListItem> GroupTypes { get; set; }
    }
}