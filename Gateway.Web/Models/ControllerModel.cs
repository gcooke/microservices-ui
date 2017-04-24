using System;
using System.Collections;
using System.Collections.Generic;

namespace Gateway.Web.Models
{
    public class ControllerModel
    {
        public ControllerModel()
        {
            VersionSummary = new List<InfoItem>();
        }

        public string Name { get; set; }
        public int TotalCalls { get; set; }
        public int TotalErrors { get; set; }
        public string AverageResponse { get; set; }

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