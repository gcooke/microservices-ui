using System;
using System.Collections.Generic;
using Gateway.Web.Models.Pdc;

namespace Gateway.Web.Services.Pdc
{
    public interface IPdcService
    {
        IEnumerable<PdcServiceModel> PingAll(IEnumerable<PdcServiceModel> services);
        PdcServiceModel[] GetInstances();
        PdcTradesModel GetTradesSummary(DateTime asOf);
    }
}