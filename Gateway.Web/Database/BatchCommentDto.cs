using System;

namespace Gateway.Web.Database
{
    public class BatchCommentDto
    {
        public int Id { get; set; }
        public int BatchStatId { get; set; }
        public Guid RequestCorrelationId { get; set; }
        public Guid ParentRequestCorrelationId { get; set; }
        public int BatchCommentType { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDescription { get; set; }
        public string ReportedBy { get; set; }
        public string Controller { get; set; }
        public string ControllerVersion { get; set; }
        public DateTime ReportedAt { get; set; }
        public string Resource { get; set; }
        public int? ParentBatchCommentId { get; set; }
    }
}