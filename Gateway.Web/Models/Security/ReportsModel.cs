using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Security
{
    [XmlType("Report")]
    public class ReportsModel
    {
        public ReportsModel()
        {
            TablesList = new List<string>();
        }

        public ReportsModel(string title)
            :this()
        {
            Name = title;
        }

        public string Name { get; set; }

        [XmlIgnore]
        public IEnumerable<ICube> Tables
        {
            get
            {
                foreach (var item in TablesList)
                {
                    var bytes = Convert.FromBase64String(item);
                    yield return CubeBuilder.FromBytes(bytes);
                }
            }
        }

        public List<string> TablesList { get; set; }

        public string Title
        {
            get { return Name.AddWordspaces(); }
        }

        public string ParameterName { get; set; }
        public string Parameter { get; set; }
        public bool SupportsParameter { get; set; }

        public void Add(ICube cube)
        {
            var bytes = cube.ToBytes();
            TablesList.Add(Convert.ToBase64String(bytes));
        }
    }    
}