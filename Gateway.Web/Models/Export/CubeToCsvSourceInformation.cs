using System.Collections.Generic;

namespace Gateway.Web.Models.Export
{
    public class CubeToCsvSourceInformation
    {
        public string Controller { get; set; }

        public string ExpectedResponseType { get; set; }

        public string Query { get; set; }

        public string Verb { get; set; }
        public List<ArgumentDto.Argument> Arguments { get; set; }
    }
}