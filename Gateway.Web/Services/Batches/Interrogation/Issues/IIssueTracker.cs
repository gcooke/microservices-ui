using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues;
using Gateway.Web.Services.Batches.Interrogation.Models;
﻿using System.Collections.Generic;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;

namespace Gateway.Web.Services.Batches.Interrogation.Issues
{
    public interface IIssueTracker<in T>
    {
        Models.Issues Identify(GatewayEntities gatewayDb, Entities pnrFoDb, T item, BatchRun run);
        IEnumerable<string> GetDescriptions();
        int GetSequence();
        void SetContext(BatchInterrogationContext context);
    }
}
