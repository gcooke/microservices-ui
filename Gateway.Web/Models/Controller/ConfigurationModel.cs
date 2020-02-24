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
            Automatic = 1,

            [XmlEnum(Name = "2")]
            Fixed = 2,

            [XmlEnum(Name = "3")]
            Container = 3
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
        [Display(Name = "Time to live whilst idle (ms)")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter the time to live.")]
        public int? TimeToLiveSec { get; set; }

        [XmlElement]
        [Display(Name = "Single work item timeout (ms)")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter the timeout.")]
        public int? TimeoutMilliSec { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Scaling Strategy")]
        public ScalingStrategies ScalingStrategyId { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Maximum worker priority")]
        [Range(1, 20, ErrorMessage = "Please enter the maximum priority.")]
        public int MaxPriority { get; set; }

        [XmlElement]
        [Required]
        [Display(Name = "Maximum workers per priority level")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter the maximum number of workers.")]
        public int MaxInstances { get; set; }

        [XmlElement]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Configuration { get; set; }

        [XmlArray(ElementName = "Versions")]
        [XmlArrayItem(ElementName = "Version", Namespace = "Gateway.Web.Models.Controller")]
        public List<Version> Versions { get; set; }

        [XmlElement]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Controller Server Restrictions")]
        public List<ControllerServer> ControllerServerRestrictions { get; set; }

        [XmlElement]
        [Display(Name = "Running workers per server")]
        public List<PriorityLimit> PriorityLimits { get; set; }

        [XmlElement]
        [Display(Name = "Caching Enabled")]
        public bool CachingEnabled { get; set; }

        [XmlElement]
        [Display(Name = "Nested Priority")]
        public bool NestedPriority { get; set; }
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