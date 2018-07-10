using System.Collections.Generic;
using Gateway.Web.Enums;
using Gateway.Web.Services.Monitoring;

namespace Gateway.Web.Models.Monitoring
{
    public class RiskReportMetrics
    {
        public string System { get; set; }
        public string ReportCategory { get; set; }
        public string ReportSubCategory { get; set; }
        public string ReportName { get; set; }
        public RiskReportResponse TMinus1 { get; set; }
        public RiskReportResponse TMinus2 { get; set; }

        public MonitoringStatus Status
        {
            get
            {
                if (TMinus1 != null)
                {
                    if (!TMinus1.Successful)
                        return MonitoringStatus.Issue;

                    if (!TMinus1.RowCount.HasValue)
                        return MonitoringStatus.Issue;

                    if (TMinus1.RowCount == 0)
                        return MonitoringStatus.Issue;

                    if (TMinus1.RowCount < (0.5 * TMinus2.RowCount ?? 0))
                        return MonitoringStatus.Warning;
                }

                return MonitoringStatus.Ok;
            }
        }

        public IDictionary<string, string> Parameters { get; set; }
    }
}