using Gateway.Web.Utils;
using System;

namespace Gateway.Web.Models.Controller
{
    public class DeepDiveDto
    {
        public Nullable<System.Guid> CorrelationId { get; set; }
        public string Controller { get; set; }
        public string Resource { get; set; }
        public Nullable<int> ResultCode { get; set; }
        private string _resultMessage;

        public string ResultMessage
        {
            get
            {
                return string.IsNullOrEmpty(_resultMessage) ? _resultMessage : _resultMessage.Replace(",", ", ");
            }
            set
            {
                _resultMessage = value;
            }
        }

        public Nullable<System.Guid> ParentCorrelationId { get; set; }
        public byte[] Request { get; set; }
        public byte[] Payload { get; set; }
        public Nullable<int> Depth { get; set; }
        public Nullable<long> PayloadId { get; set; }
        public Nullable<long> PayloadRequestId { get; set; }
        public string PayloadType { get; set; }
        public string PayloadTypeRequest { get; set; }
        public Nullable<int> TimeTakenInMs { get; set; }

        public string ResourceDisplay
        {
            get
            {
                return Resource?.Length > 100 ? Resource.Substring(0, 50) + " ..." : Resource;
            }
        }

        public string TimeTakenFormatted
        {
            get { return TimeTakenInMs.HasValue ? TimeTakenInMs.Value.FormatTimeTaken(true) : null; }
        }
    }
}