using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Utils
{
    public static class SelectItemListEx
    {
        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<BusinessFunction> items)
        {
            return items.Select(x => new SelectListItem
            {
                Text = string.Format("{0} ({1})", x.Name, x.GroupType),
                Value = x.Id.ToString()
            });
        }

        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<GroupType> items)
        {
            return items.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }

    }
}