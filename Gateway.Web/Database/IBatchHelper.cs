using System;
using System.Threading.Tasks;

namespace Gateway.Web.Database
{
    public interface IBatchHelper
    {
        Task<RiskBatchModel> GetRiskBatchReportModel(DateTime reportDate, string targetSite);
    }
}