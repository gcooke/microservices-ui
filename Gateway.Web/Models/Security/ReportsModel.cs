using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Security
{
    [XmlType("Report")]
    public class ReportsModel
    {
        public ReportsModel()
        {
            Tables = new List<ReportTable>();
        }

        public ReportsModel(string report) : this()
        {
            Name = report;
        }

        public string Name { get; set; }

        public string Title
        {
            get { return Name.AddWordspaces(); }
        }

        public string ParameterName { get; set; }
        public string Parameter { get; set; }
        public bool SupportsParameter { get; set; }
        public List<ReportTable> Tables { get; private set; }
    }

    [XmlType("Table")]
    public class ReportTable
    {
        public ReportTable()
        {
            Rows = new List<ReportRows>();
            Columns = new List<string>();
        }
        public string Title { get; set; }
        public bool PivotColumnHeaders { get; set; }
        public List<string> Columns { get; private set; }
        public List<ReportRows> Rows { get; private set; }
    }

    [XmlType("Row")]
    public class ReportRows
    {
        public ReportRows()
        {
            Values = new List<string>();
        }
        public List<string> Values { get; private set; }
    }
}