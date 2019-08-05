using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Export
{
    public class ExportDetailViewModel
    {
        public ExportDetailViewModel()
        {
            FileExportsHistory = new List<FileExportsHistory>();
        }

        public DateTime BusinessDate { get; set; }

        public FileExport FileExport { get; set; }
        public IList<FileExportsHistory> FileExportsHistory { get; set; }
    }
}