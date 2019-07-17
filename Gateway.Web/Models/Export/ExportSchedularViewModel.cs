using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Export
{
    public class ExportSchedularViewModel
    {
        public ExportSchedularViewModel()
        {
            ExportCrons = new List<ExportCRONGroup>();
        }

        public IList<ExportCRONGroup> ExportCrons { get; set; }
        public DateTime BusinessDate { get; set; }
    }
}