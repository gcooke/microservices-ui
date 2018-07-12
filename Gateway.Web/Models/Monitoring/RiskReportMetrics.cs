using System.Collections.Generic;
using System.Linq;
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
        public RiskReportResponse Results { get; set; }
        public RiskReportResponse PreviousResults { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string Identifier => $"{System}-{ReportCategory}-{ReportSubCategory}-{ReportName}-{ParameterList}";

        public string ParameterList
        {
            get { return string.Join("|", Parameters.Select(x => $"{x.Key}={x.Value}").ToList()); }
        }


        public MonitoringStatus Status
        {
            get
            {
                if (Results != null)
                {
                    if (!Results.Successful)
                        return MonitoringStatus.Issue;

                    if (!Results.RowCount.HasValue)
                        return MonitoringStatus.Issue;

                    if (Results.RowCount == 0)
                        return MonitoringStatus.Issue;

                    if (PreviousResults != null && Results.RowCount < (0.5 * PreviousResults.RowCount ?? 0))
                        return MonitoringStatus.Warning;

                    return MonitoringStatus.Ok;
                }

                return MonitoringStatus.Issue;
            }
        }
    }
}