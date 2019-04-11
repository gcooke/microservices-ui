using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Models.Interrogation;

namespace Gateway.Web.Services
{
    public interface IRiskBatchInterrogationService
    {
        RiskBatchModel Analyze(string batch, DateTime date);
    }
}