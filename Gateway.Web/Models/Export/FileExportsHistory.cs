using System;

namespace Gateway.Web.Models.Export
{
    public class FileExportsHistory
    {
        public long Id { get; set; }
        public long ExportId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public bool EmailSent { get; set; }
        public bool IsForced { get; set; }
        public string TriggeredBy { get; set; }
        public DateTime ValuationDate { get; set; }
    }
}