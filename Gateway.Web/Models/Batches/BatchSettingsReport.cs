using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bagl.Cib.MIT.StatePublisher;
using CronExpressionDescriptor;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Batches
{
    public class BatchSettingsReport
    {
        public BatchSettingsReport()
        {
            Batches = new List<BatchItem>();
        }

        public List<BatchItem> Batches { get; set; }
    }

    public class BatchItem
    {
        public BatchItem(string name)
        {
            Name = name;
            Differences = new List<string>();
        }
        public string Name { get; set; }

        public string BatchName
        {
            get
            {
                var index = Name.IndexOf("-");
                if (index < 0) return Name;
                return Name.Substring(0, index).Trim();
            }
        }

        public string Schedule { get; set; }
        public EchoCreationDto Echo { get; set; }
        public string MarketDataMap { get; set; }
        public List<string> Differences { get; set; }
    }
}