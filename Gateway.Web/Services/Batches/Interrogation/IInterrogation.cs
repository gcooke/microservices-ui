using System;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Services.Batches.Interrogation
{
    public interface IInterrogation
    {
        ReportsModel Run(DateTime valuationDate);
    }
}