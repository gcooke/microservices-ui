using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation;

namespace Gateway.Web.Services
{
    public class RiskBatchInterrogationService : IRiskBatchInterrogationService
    {
        public RiskBatchInterrogationService(ISystemInformation information)
        {
            ConnectionString = information.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public string ConnectionString { get; set; }

        public void PopulateLookups(InterrogationModel model)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var tradeSources = db
                    .RiskBatchSchedules
                    .Where(x => x.TradeSourceType == "Site")
                    .Select(x => x.TradeSource)
                    .Distinct()
                    .ToList()
                    .Select(x => new SelectListItem() { Value = x, Text = x });

                var batchTypes = db.RiskBatchSchedules
                    .Select(x => x.RiskBatchConfiguration.Type)
                    .Distinct()
                    .ToList()
                    .Select(x => new SelectListItem() { Value = x, Text = x});

                model.TradeSources.AddRange(tradeSources);
                model.BatchTypes.AddRange(batchTypes);
            }
        }

        public void Analyze(InterrogationModel model)
        {
            var actualDate = GetPreviousWorkDay(model.ReportDate);

            throw new NotImplementedException();
        }

        private DateTime GetPreviousWorkDay(DateTime date)
        {
            date = date.AddDays(-1);
            while (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                date = date.AddDays(-1);
            return date;
        }
    }
}