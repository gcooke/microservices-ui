using System.Collections.Generic;

namespace Gateway.Web.Models.Export
{
    public class ExportCRONGroup
    {
        public IList<FileExportViewModel> FileExports { get; set; }
        public string GroupName { get; set; }
        public string Schedule { get; set; }
    }
}