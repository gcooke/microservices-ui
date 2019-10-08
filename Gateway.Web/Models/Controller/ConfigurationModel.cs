using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Controller
{ 
    [XmlRoot(ElementName = "Controller")]
    public class ConfigurationModel
    {
        public enum ScalingStrategies
        {
            [XmlEnum(Name = "1")]
            Static = 1,

            [XmlEnum(Name = "2")]
            Dynamic =2 
        }
        
        public bool IsUpdate { get { return ControllerId != 0; } }

        [XmlAttribute(AttributeName = "Id")]
        public long ControllerId { get; set; }

        [XmlAttribute]
        [Required]
        [Display(Name = "Name")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "The controller name must contain alphabets only.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [XmlElement]
        [Display(Name = "Time to Live")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter the time to live.")]
        public int? TimeToLiveSec { get; set; }

        [XmlElement]
        [Display(Name = "Timeout")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter the timeout.")]
        public int? TimeoutMilliSec { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Scaling Strategy")]
        public ScalingStrategies ScalingStrategyId { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "MaxPriority")]
        [Range(1, 20, ErrorMessage = "Please enter the maximum priority.")]
        public int MaxPriority { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Maximum Instances")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter the number of instances.")]
        public int MaxInstances { get; set; }

        [XmlElement]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Configuration { get; set; }

        [XmlArray(ElementName="Versions")]
        [XmlArrayItem(ElementName = "Version", Namespace= "Gateway.Web.Models.Controller")]
        public List<Version> Versions { get; set; }


        [XmlElement]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Controller Server Restrictions")]
        public List<ControllerServer> ControllerServerRestrictions { get; set; }

        [XmlElement] 
        [Display(Name = "Priority Limits")]
        public List<PriorityLimit> PriorityLimits { get; set; }

    }

    public class PriorityLimit
    {
        [XmlElement]
        public int? Id { get; set; }

        [XmlElement]
        [Display(Name = "Enabled")]
        public bool Enabled { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Priority")]
        [Range(0, 20, ErrorMessage = "Invalid Priority")]
        public int Priority { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Instances")]
        [Range(1, 2000, ErrorMessage = "Please enter the maximum priority.")]
        public int Instances { get; set; }
    }
}