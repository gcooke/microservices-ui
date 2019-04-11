using System;
using System.Collections.Generic;
using Gateway.Web.Services.Batches.Interrogation.Models;

namespace Gateway.Web.Services.Batches.Interrogation.Services.BatchService
{
    public interface IBatchService
    {
        IEnumerable<Batch> GetBatchesForDate(DateTime date);
    }
}
