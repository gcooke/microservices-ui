using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Enums;

namespace Gateway.Web.Models.Monitoring
{
    public class RiskReportMetricsViewModel
    {
        public DateTime BusinessDate { get; set; }
        public List<GroupedRiskReportMetrics> Metrics { get; set; }
    }

    public class GroupedRiskReportMetrics
    {
        public string System { get; set; }
        public string ReportCategory { get; set; }
        public string ReportSubCategory { get; set; }
        public string ReportName { get; set; }

        public string Identifier => $"{System}-{ReportCategory}-{ReportSubCategory}-{ReportName}";

        public int ResultCount
        {
            get { return Metrics.Where(x => x.Results.RowCount.HasValue).Sum(x => x.Results.RowCount.Value); }
        }

        public int PreviousResultsCount
        {
            get { return Metrics.Where(x => x.PreviousResults?.RowCount != null).Sum(x => x.PreviousResults.RowCount.Value); }
        }

        public MonitoringStatus Status
        {
            get
            {
                var okCount = Metrics.Count(x => x.Status == MonitoringStatus.Ok);
                var issueCount = Metrics.Count(x => x.Status == MonitoringStatus.Issue);
                var totalCount = Metrics.Count;

                if (okCount == totalCount)
                    return MonitoringStatus.Ok;

                if (issueCount == totalCount)
                    return MonitoringStatus.Issue;

                return MonitoringStatus.Warning;
            }
        }

        public List<RiskReportMetrics> Metrics { get; set; }
    }
}