using System.Xml.Serialization;

namespace Gateway.Web.Services
{
    [XmlType("Process")]
    public class ProcessInformation
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string StartTime { get; set; }

        [XmlAttribute]
        public int PID { get; set; }

        [XmlAttribute]
        public long WorkingSet { get; set; }

        [XmlIgnore]
        public string WorkingSetLabel
        {
            get
            {
                if (WorkingSet <= 0) return "";

                return string.Format("{0} Mb", (WorkingSet / 1024) / 1024.0);
            }
        }

        [XmlAttribute]
        public int Threads { get; set; }
    }
}