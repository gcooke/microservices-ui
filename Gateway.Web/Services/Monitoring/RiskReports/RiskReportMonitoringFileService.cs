using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bagl.Cib.MIT.IoC;
using CsvHelper;
using Gateway.Web.Models;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Services.Monitoring.RiskReports.Config;

namespace Gateway.Web.Services.Monitoring.RiskReports
{
    public class RiskReportMonitoringFileService : IRiskReportMonitoringFileService
    {
        private readonly ISystemInformation _systemInformation;

        public RiskReportMonitoringFileService(ISystemInformation systemInformation)
        {
            _systemInformation = systemInformation;
        }

        public IList<RiskReportMonitoringConfigItem> GetConfig()
        {
            var configPath = _systemInformation.GetSetting("RiskReportMonitoringConfigFilePath");

            using (TextReader fileReader = File.OpenText(configPath))
            {
                var csv = new CsvReader(fileReader);
                csv.Configuration.RegisterClassMap<ConfigMap>();
                csv.Configuration.Delimiter = ";";
                var configs = csv.GetRecords<RiskReportMonitoringConfigItem>().ToList();
                return configs;
            }
        }
    }
}