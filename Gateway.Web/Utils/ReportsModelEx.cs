using System.Collections.Generic;
using System.Web.Mvc;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Utils
{
    public static class ReportsModelEx
    {
        public static ICube ConvertToReportTable(this List<SelectListItem> input)
        {
            var result = new CubeBuilder()
                .AddColumn("Add-In")
                .AddColumn("Version")
                .Build();
            result.SetAttribute("Title", "");

            foreach (var item in input)
            {
                var values = item.Value.Split('|');
                result.AddRow(values);
            }
            return result;
        }
    }
}