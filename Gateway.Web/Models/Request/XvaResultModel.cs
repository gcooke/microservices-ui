using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Bagl.Cib.MIT.Cube;

namespace Gateway.Web.Models.Request
{
    public class XvaResultModel
    {
        public IDictionary<string, string> HeaderItems { get; set; }
        public IDictionary<string, string> Reports { get; set; }
        public CubeModel BatchStatistics { get; set; }
        public string BatchStatisticsRawData { get; set; }

        public XvaResultModel(string data)
        {
            HeaderItems = new Dictionary<string, string>();
            Reports = new Dictionary<string, string>();
            Populate(data);
        }

        private void Populate(string data)
        {
            var xElement = GetXElement(data);
            AddHeaderItems(xElement);
            AddReports(xElement);
        }

        private void AddReports(XElement xElement)
        {
            var reports = xElement.Element("Reports");
            if (reports == null) return;
            foreach (var report in reports.Elements())
            {
                var name = report.Element("Name");
                var data = report.Element("Data");
                if (name == null) continue;
                if (data == null) continue;

                if (name.Value.Equals("batch statistics", StringComparison.InvariantCultureIgnoreCase))
                {
                    BatchStatisticsRawData = data.Value;
                    var bytes = Convert.FromBase64String(data.Value);
                    var cube = CubeBuilder.FromBytes(bytes);
                    BatchStatistics = new CubeModel(cube, "Batch Statistics");
                }
                else
                {
                    Reports.Add(name.Value, data.Value);
                }
            }
        }

        private void AddHeaderItems(XElement data)
        {
            if (data.Elements().Any())
            {
                foreach (var element in data.Elements())
                {
                    if (element.Name.LocalName.Equals("Reports")) continue;
                    AddHeaderItems(element);
                }

                return;
            }

            HeaderItems.Add(data.Name.ToString(), data.Value);
        }

        private XElement GetXElement(string data)
        {
            var xml = XElement.Load(new StringReader(data));
            var isScenarioResult = xml.Name.ToString().Equals("ScenarioResult", StringComparison.InvariantCultureIgnoreCase);

            if (!isScenarioResult)
                throw new Exception("Unable to process payload.");

            return xml;
        }

    }
}