using System;
using System.Collections.Generic;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation;

namespace Gateway.Web.Services
{
    public class RiskBatchInterrogationService : IRiskBatchInterrogationService
    {
        //private readonly Dictionary<string, IInterrogation> _interrogations;
        //private readonly IInterrogation _fallback;

        public RiskBatchInterrogationService(ISystemInformation information)
        {
            //_interrogations = new Dictionary<string, IInterrogation>(StringComparer.CurrentCultureIgnoreCase);
            //_interrogations.Add("Counterparty.PFE", information.Resolve<PfeInterrogation>());

            //_fallback = information.Resolve<BaseInterrogation>();
        }

        public RiskBatchModel Analyze(string batch, DateTime date)
        {
            var result = new RiskBatchModel(batch, date);
            var actualDate = GetPreviousWorkDay(date);

            //IInterrogation interrogation;
            //if (!_interrogations.TryGetValue(batch, out interrogation))
            //{
            //    interrogation = _fallback;
            //}

            //result.Report = interrogation.Run(actualDate);
            return result;
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