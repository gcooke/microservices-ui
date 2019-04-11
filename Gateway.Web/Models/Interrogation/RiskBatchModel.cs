using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Models.Interrogation
{
    public class RiskBatchModel
    {
        public RiskBatchModel(string batch, DateTime actualdate)
        {
            Name = batch;
            ReportDate = actualdate;
            Report = new ReportsModel();
        }

        public string Name { get; }
        public DateTime ReportDate { get; }
        public ReportsModel Report { get; set; }
    }
}