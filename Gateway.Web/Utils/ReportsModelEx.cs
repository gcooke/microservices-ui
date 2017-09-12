using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Utils
{
    public static class ReportsModelEx
    {
        public static ReportTable ConvertToReportTable(this List<SelectListItem> input)
        {
            var result = new ReportTable();

            result.Columns.Add("Add-In");
            result.Columns.Add("Version");

            foreach (var item in input)
            {
                var row = new ReportRows();
                var values = item.Value.Split('|');
                row.Values.Add(values[0]);
                row.Values.Add(values[1]);
                result.Rows.Add(row);
            }
            return result;
        }
    }
}