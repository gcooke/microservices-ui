using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Monitoring
{
    public class RiskReportMetricsViewModel
    {
        public DateTime BusinessDate { get; set; }
        public IList<RiskReportMetrics> Metrics { get; set; }
    }
}