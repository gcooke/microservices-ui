//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gateway.Web.Database
{
    using System;
    
    public partial class spGetPayloadResponsesByController_Result
    {
        public long Id { get; set; }
        public System.Guid CorrelationId { get; set; }
        public string Direction { get; set; }
        public string PayloadType { get; set; }
        public string CompressionType { get; set; }
        public byte[] Data { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public Nullable<long> DataLengthBytes { get; set; }
        public string Server { get; set; }
    }
}