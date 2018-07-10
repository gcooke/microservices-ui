using CsvHelper.Configuration;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports.Config
{
    public class ConfigMap : ClassMap<RiskReportMonitoringConfigItem>
    {
        public ConfigMap()
        {
            Map(m => m.System).Name("System").Index(0);
            Map(m => m.ReportCategory).Name("ReportCategory").Index(1);
            Map(m => m.ReportSubCategory).Name("ReportSubCategory").Index(2);
            Map(m => m.ReportName).Name("ReportName").Index(3);
            Map(m => m.Controller).Name("Controller").Index(4);
            Map(m => m.DateFormat).Name("DateFormat").Index(5);
            Map(m => m.DateParameter).Name("DateParameter").Index(6);
            Map(m => m.RequestClient).Name("RequestClient").Index(7);
            Map(m => m.ResolveDataSet).Name("ResolveDataSet").Index(8);
            Map(m => m.DataSetNamePrefix).Name("DataSetNamePrefix").Index(9);
            Map(m => m.DataSetParameter).Name("DataSetParameter").Index(10);
            Map(m => m.Endpoint).Name("Endpoint").Index(11);
            Map(m => m.Parameters).Name("Parameters").Index(12);
        }
    }
}