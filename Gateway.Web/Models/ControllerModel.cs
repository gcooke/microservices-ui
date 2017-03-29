using System.Collections.Generic;

namespace Gateway.Web.Models
{
    public class ControllerModel
    {
        public ControllerModel()
        {
            RequestSummary = new List<InfoItem>();
            VersionSummary = new List<InfoItem>();
        }

        public string Name { get; set; }

        public List<InfoItem> RequestSummary { get; private set; }

        public List<InfoItem> VersionSummary { get; private set; }
    }

    public class InfoItem
    {
        public InfoItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}