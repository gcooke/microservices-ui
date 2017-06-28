using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Controller
{
    [XmlRoot(ElementName = "Controller")]
    public class ConfigurationModel
    {
        public ConfigurationModel(string name)
        {
            Name = name;
        }

        public ConfigurationModel() : this("") { }

        public enum ScalingStrategies
        {
            Local = 1,
            Remote = 2
        }

        [XmlAttribute]
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [XmlElement]
        [Display(Name = "Time to Live")]
        public int? TimeToLiveSec { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Scaling Strategy")]
        public ScalingStrategies ScalingStrategy { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Weighting")]
        public int Weighting { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Maximum Instances")]
        public int MaxInstances { get; set; }
    }
}