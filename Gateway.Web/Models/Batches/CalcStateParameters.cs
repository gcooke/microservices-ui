using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Batches
{
    /// <summary>
    /// Duplicate of Bagl.CIB.Sigma.Models.CalculationParameters. Please see EchoCreationDto comment.
    /// </summary>
    public class CalcStateParameters
    {
        public CalcStateParameters()
        {
            Items = new List<CalcStateParameter>();
        }

        public string BatchName { get; set; }
        public List<CalcStateParameter> Items { get; set; }

        public override string ToString()
        {
            return BatchName;
        }
    }

    [Serializable]
    public class CalcStateParameter
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        [XmlIgnore]
        public object[] Values { get; set; }

        [XmlIgnore]
        public string DisplayGroup => Path.Contains(".") ? Path.Substring(0, Path.IndexOf(".")) : null;

        [XmlIgnore]
        public string DisplayName => Path.Contains(".") ? Path.Substring(Path.IndexOf(".") + 1) : Path;

        public override string ToString()
        {
            return $"{Path}={Value}";
        }
    }
}