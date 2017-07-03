namespace Gateway.Web.Models.Security
{
    public class ReportsModel
    {
        public ReportsModel(string report)
        {
            Report = report;
        }

        public string Report { get; set; }
        public string ReportName { get { return Report; } }
    }
}