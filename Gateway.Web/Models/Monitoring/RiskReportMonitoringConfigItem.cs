namespace Gateway.Web.Models.Monitoring
{
    public class RiskReportMonitoringConfigItem
    {
        public string System { get; set; }
        public string ReportCategory { get; set; }
        public string ReportSubCategory { get; set; }
        public string ReportName { get; set; }
        public string Controller { get; set; }
        public string Endpoint { get; set; }
        public string DateParameter { get; set; }
        public string DateFormat { get; set; }
        public string RequestClient { get; set; }
        public bool ResolveDataSet { get; set; }
        public string DataSetNamePrefix { get; set; }
        public string DataSetParameter { get; set; }
        public string Parameters { get; set; }
    }
}