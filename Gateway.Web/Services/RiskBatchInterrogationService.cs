using System;
using System.Collections.Generic;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation;

namespace Gateway.Web.Services
{
    public class RiskBatchInterrogationService : IRiskBatchInterrogationService
    {
        public RiskBatchInterrogationService(ISystemInformation information)
        {
            
        }
        
        public void PopulateLookups(InterrogationModel model)
        {
            
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