using System.Collections.Generic;

namespace Gateway.Web.Models.Export
{
    public class CubeToCsvDestinationInformation
    {
        public string DestinationUrl { get; set; }
        public string FileName { get; set; }
        public List<ArgumentDto.Argument> Arguments { get; set; }
    }
}