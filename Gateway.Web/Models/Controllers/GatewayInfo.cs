using System.Linq;

namespace Gateway.Web.Models.Controllers
{

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models", IsNullable = false)]
    public partial class GatewayInfo
    {

        private GatewayInfoGatewayNode[] gatewayNodesField;

        private ControllerInformation[] servicesField;


        [System.Xml.Serialization.XmlArrayItemAttribute("GatewayNode", IsNullable = false)]
        public GatewayInfoGatewayNode[] GatewayNodes
        {
            get
            {
                return this.gatewayNodesField;
            }
            set
            {
                this.gatewayNodesField = value;
            }
        }


        [System.Xml.Serialization.XmlArrayItemAttribute("ControllerInformation", Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Services.Registry", IsNullable = false)]
        public ControllerInformation[] Services
        {
            get
            {
                return this.servicesField;
            }
            set
            {
                this.servicesField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public partial class GatewayInfoGatewayNode
    {

        private string addressField;

        private string idField;

        private string nodeField;

        private GatewayInfoGatewayNodePerformanceCounters performanceCountersField;

        private GatewayInfoGatewayNodeProcesses processesField;

        private GatewayInfoGatewayNodeQueues queuesField;


        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }


        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }


        public string Node
        {
            get
            {
                return this.nodeField;
            }
            set
            {
                this.nodeField = value;
            }
        }


        public GatewayInfoGatewayNodePerformanceCounters PerformanceCounters
        {
            get
            {
                return this.performanceCountersField;
            }
            set
            {
                this.performanceCountersField = value;
            }
        }


        public GatewayInfoGatewayNodeProcesses Processes
        {
            get
            {
                return this.processesField;
            }
            set
            {
                this.processesField = value;
            }
        }


        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public GatewayInfoGatewayNodeQueues Queues
        {
            get
            {
                return this.queuesField;
            }
            set
            {
                this.queuesField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public partial class GatewayInfoGatewayNodePerformanceCounters
    {

        private decimal cpuUsageField;

        private ushort memUsageField;


        public decimal CpuUsage
        {
            get
            {
                return this.cpuUsageField;
            }
            set
            {
                this.cpuUsageField = value;
            }
        }


        public ushort MemUsage
        {
            get
            {
                return this.memUsageField;
            }
            set
            {
                this.memUsageField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts", IsNullable = false)]
    public partial class InformationServiceSet
    {

        private InformationServiceSetInformationCheck[] checksField;

        private InformationServiceSetNode nodeField;

        private InformationServiceSetService serviceField;


        [System.Xml.Serialization.XmlArrayItemAttribute("InformationCheck", IsNullable = false)]
        public InformationServiceSetInformationCheck[] Checks
        {
            get
            {
                return this.checksField;
            }
            set
            {
                this.checksField = value;
            }
        }


        public InformationServiceSetNode Node
        {
            get
            {
                return this.nodeField;
            }
            set
            {
                this.nodeField = value;
            }
        }


        public InformationServiceSetService Service
        {
            get
            {
                return this.serviceField;
            }
            set
            {
                this.serviceField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    public partial class InformationServiceSetInformationCheck
    {

        private string checkIDField;

        private string nameField;

        private string outputField;

        private string statusField;


        public string CheckID
        {
            get
            {
                return this.checkIDField;
            }
            set
            {
                this.checkIDField = value;
            }
        }


        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }


        public string Output
        {
            get
            {
                return this.outputField;
            }
            set
            {
                this.outputField = value;
            }
        }


        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    public partial class InformationServiceSetNode
    {

        private string addressField;

        private string nodeField;


        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }


        public string Node
        {
            get
            {
                return this.nodeField;
            }
            set
            {
                this.nodeField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    public partial class InformationServiceSetService
    {

        private string addressField;

        private string idField;

        private ushort portField;

        private string serviceField;

        private InformationServiceSetServiceTags tagsField;


        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }


        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }


        public ushort Port
        {
            get
            {
                return this.portField;
            }
            set
            {
                this.portField = value;
            }
        }


        public string Service
        {
            get
            {
                return this.serviceField;
            }
            set
            {
                this.serviceField = value;
            }
        }


        public InformationServiceSetServiceTags Tags
        {
            get
            {
                return this.tagsField;
            }
            set
            {
                this.tagsField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    public partial class InformationServiceSetServiceTags
    {

        private string stringField;


        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public string @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Services.Registry")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Services.Registry", IsNullable = false)]
    public partial class ControllerInformation
    {

        private string nameField;

        private InformationServiceSet[] versionsField;


        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }


        [System.Xml.Serialization.XmlArrayItemAttribute("InformationServiceSet", Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts", IsNullable = false)]
        public InformationServiceSet[] Versions
        {
            get
            {
                return this.versionsField;
            }
            set
            {
                this.versionsField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public partial class GatewayInfoGatewayNodeProcesses
    {

        private GatewayInfoGatewayNodeProcessesProcess[] processField;


        [System.Xml.Serialization.XmlArrayItemAttribute("Process", IsNullable = false)]
        public GatewayInfoGatewayNodeProcessesProcess[] Process
        {
            get
            {
                return this.processField;
            }
            set
            {
                this.processField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public partial class GatewayInfoGatewayNodeProcessesProcess
    {

        private string argsField;

        private string nameField;

        private ushort pIDField;

        private System.DateTime startTimeField;

        private uint workingSetField;


        public string Args
        {
            get
            {
                return this.argsField;
            }
            set
            {
                this.argsField = value;
            }
        }


        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }


        public ushort PID
        {
            get
            {
                return this.pIDField;
            }
            set
            {
                this.pIDField = value;
            }
        }


        public System.DateTime StartTime
        {
            get
            {
                return this.startTimeField;
            }
            set
            {
                this.startTimeField = value;
            }
        }


        public uint WorkingSet
        {
            get
            {
                return this.workingSetField;
            }
            set
            {
                this.workingSetField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public partial class GatewayInfoGatewayNodeQueues
    {

        private GatewayInfoGatewayNodeQueuesControllerQueueInfo[] controllerQueueInfosField;

        private string nodeField;


        [System.Xml.Serialization.XmlArrayItemAttribute("ControllerQueueInfo", IsNullable = false)]
        public GatewayInfoGatewayNodeQueuesControllerQueueInfo[] ControllerQueueInfos
        {
            get
            {
                return this.controllerQueueInfosField;
            }
            set
            {
                this.controllerQueueInfosField = value;
            }
        }


        public string Node
        {
            get
            {
                return this.nodeField;
            }
            set
            {
                this.nodeField = value;
            }
        }
    }


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public partial class GatewayInfoGatewayNodeQueuesControllerQueueInfo
    {

        private System.DateTime lastDequeueField;

        private System.DateTime lastEnqueueField;

        private byte lengthField;

        private string nameField;

        private string versionField;

        private byte workersField;


        public System.DateTime LastDequeue
        {
            get
            {
                return this.lastDequeueField;
            }
            set
            {
                this.lastDequeueField = value;
            }
        }


        public System.DateTime LastEnqueue
        {
            get
            {
                return this.lastEnqueueField;
            }
            set
            {
                this.lastEnqueueField = value;
            }
        }


        public byte Length
        {
            get
            {
                return this.lengthField;
            }
            set
            {
                this.lengthField = value;
            }
        }


        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }


        public string Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }


        public byte Workers
        {
            get
            {
                return this.workersField;
            }
            set
            {
                this.workersField = value;
            }
        }
    }

    public class WorkerInfo
    {
        public WorkerInfo(InformationServiceSet informationServiceSet)
        {
            Id = informationServiceSet.Service.ID;
            Node = informationServiceSet.Service.Address;
            Service = informationServiceSet.Service.Service;

            var check = informationServiceSet.Checks.FirstOrDefault(c => c.CheckID == "serfHealth");
            if (check == null) return;

            Status = check.Status;
            Output = check.Output;
        }

        public WorkerInfo()
        {
        }

        public string Pid { get { return (Id.Split('|').Any()) ? Id.Split('|')[1] : string.Empty; } }
        public string Id { get; set; }
        public string Service { get; set; }
        public string Version { get { return (Service.Split('/').Any()) ? Service.Split('/')[1] : string.Empty; } }
        public string Node { get; set; }
        public string Status { get; set; }
        public string Output { get; set; }
        public bool State
        {
            get { return Status != "Passing"; }
        }
    }
}