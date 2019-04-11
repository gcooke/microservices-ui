using System;

namespace Gateway.Web.Services.Batches.Interrogation.Attributes
{
    public class AppliesToBatchAttribute : Attribute
    {
        public Models.Enums.Batches Batch { get; }

        public AppliesToBatchAttribute(Models.Enums.Batches batch)
        {
            Batch = batch;
        }
    }
}
