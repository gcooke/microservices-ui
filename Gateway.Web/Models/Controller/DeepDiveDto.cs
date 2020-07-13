using System;

namespace Gateway.Web.Models.Controller
{
    public class DeepDiveDto
    {
        public Nullable<System.Guid> CorrelationId { get; set; }
        public string Controller { get; set; }
        public string Resource { get; set; }
        public Nullable<int> ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public Nullable<System.Guid> ParentCorrelationId { get; set; }
        public byte[] Payload { get; set; }
        public Nullable<int> Depth { get; set; }

        public string ResourceDisplay
        {
            get
            {
                return Resource?.Length > 100 ? Resource.Substring(0, 50) + " ..." : Resource;
            }
        }
    }
}