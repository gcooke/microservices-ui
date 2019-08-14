using System;

namespace Gateway.Web.Models.Export
{
    public class FetchExportCube
    {
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Type { get; set; }
        public string Schedule { get; set; }

        public DateTime StartDateTime { get; set; }

        public bool IsDisabled { get; set; }
        public bool? IsSuccessful { get; set; }
        public string Message { get; set; }

        public string TriggeredBy { get; set; }

        public bool IsForced { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public long ExportId { get; set; }
        public long FileExportsHistoryId { get; set; }
    }
}