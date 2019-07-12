using System;

namespace Gateway.Web.Models.Export
{
    public class CubeToCsvExportDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
        public DateTime StartDateTime { get; set; }
        public CubeToCsvSourceInformation SourceInfo { get; set; }
        public CubeToCsvDestinationInformation DestinationInfo { get; set; }
    }
}