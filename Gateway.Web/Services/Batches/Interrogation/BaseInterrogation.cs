using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Services.Batches.Interrogation
{
    public class BaseInterrogation : IInterrogation
    {
        public virtual ReportsModel Run(DateTime valuationDate)
        {
            throw new NotImplementedException();
        }
    }
}