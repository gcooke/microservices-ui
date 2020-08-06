using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Models.Request;

namespace Gateway.Web.Models.Interrogation
{
    public class XvaReportModel
    {
        public const string DateFormat = "ddd dd MMM";
        public const string RunDateFormat = "dd/MM HH:mm";

        public XvaReportModel()
        {
            Reports = new List<AvailableReport>();
        }

        public string Date { get; set; }
        public bool IncludeAllRows { get; set; }
        public Guid CorrelationId { get; set; }
        public List<AvailableReport> Reports { get; }
        public CubeModel BatchStatistics { get; set; }

        public void AddReport(DateTime? started, DateTime businessDay, string site, Guid? correlationId)
        {
            Reports.Add(new AvailableReport(started, businessDay, site, correlationId));
        }
    }

    public class AvailableReport
    {
        public AvailableReport(DateTime? started, DateTime businessDay, string site, Guid? correlationId)
        {
            CorrelationId = correlationId ?? Guid.Empty;
            Site = site;
            BusinessDate = businessDay.ToString(XvaReportModel.DateFormat);
            RunDateTime = started?.ToString(XvaReportModel.RunDateFormat);
        }

        public Guid CorrelationId { get; }
        public string Site { get; set; }
        public string BusinessDate { get; }
        public string RunDateTime { get; }
    }
}