using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Models.Group
{
    public class GroupDetailsModel
    {
        public GroupModel Group { get; set; }
        public IList<SelectListItem> BusinessFunctions { get; set; }
    }
}