using System.Collections.Generic;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Models.Batches;

namespace Gateway.Web.Services.Batches.BatchCosts
{
    public interface IBatchCostsService
    {
        ICube GetBatchCosts();
        List<CostGroupMonthlyBatchCost> GetBatchMonthlyCosts(ICube costsCube);
    }
}
