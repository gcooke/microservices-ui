using Gateway.Web.Enums;
using System;

namespace Gateway.Web.Models.Export
{
    public class ExportSchedule
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
        public ExportType Type { get; set; }
        public CubeToCsvSourceInformation SourceInfo { get; set; }
        public CubeToCsvDestinationInformation DestinationInfo { get; set; }
        public string SuccessEmailAddress { get; set; }
        public string FailureEmailAddress { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime StartDateTime { get; set; }
    }
}