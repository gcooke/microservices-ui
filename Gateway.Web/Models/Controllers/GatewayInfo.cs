using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Controllers
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    [XmlRoot(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models", IsNullable = false)]
    public class GatewayInfo
    {
        private GatewayInfoGatewayNode[] gatewayNodesField;
        private ControllerInformation[] servicesField;

        [XmlArrayItem("GatewayNode", IsNullable = false)]
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

        [XmlArrayItem("ControllerInformation", Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Services.Registry", IsNullable = false)]
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

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public class GatewayInfoGatewayNode
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


        [XmlElementAttribute(IsNullable = true)]
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

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public class GatewayInfoGatewayNodePerformanceCounters
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

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    [XmlRoot(Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts", IsNullable = false)]
    public class InformationServiceSet
    {

        private InformationServiceSetInformationCheck[] checksField;

        private InformationServiceSetNode nodeField;

        private InformationServiceSetService serviceField;


        [XmlArrayItem("InformationCheck", IsNullable = false)]
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

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    public class InformationServiceSetInformationCheck
    {
        public string CheckID { get; set; }
        public string Name { get; set; }
        public string Output { get; set; }
        public string Status { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    public class InformationServiceSetNode
    {
        public string Address { get; set; }
        public string Node { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts")]
    public class InformationServiceSetService
    {
        public string Address { get; set; }
        public string ID { get; set; }
        public ushort Port { get; set; }
        public string Service { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Services.Registry")]
    [XmlRoot(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Services.Registry", IsNullable = false)]
    public class ControllerInformation
    {
        public string Name { get; set; }

        [XmlArrayItem("InformationServiceSet", Namespace = "http://schemas.datacontract.org/2004/07/CondenserDotNet.Core.DataContracts", IsNullable = false)]
        public InformationServiceSet[] Versions { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public class GatewayInfoGatewayNodeProcesses
    {
        [XmlArrayItem("Process", IsNullable = false)]
        public GatewayInfoGatewayNodeProcessesProcess[] Process { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public class GatewayInfoGatewayNodeProcessesProcess
    {
        public string Args { get; set; }
        public string Name { get; set; }
        public ushort PID { get; set; }
        public DateTime StartTime { get; set; }
        public uint WorkingSet { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public class GatewayInfoGatewayNodeQueues
    {
        [XmlArrayItem("ControllerQueueInfo", IsNullable = false)]
        public GatewayInfoGatewayNodeQueuesControllerQueueInfo[] ControllerQueueInfos { get; set; }
        public string Node { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
    public class GatewayInfoGatewayNodeQueuesControllerQueueInfo
    {
        public DateTime LastDequeue { get; set; }
        public DateTime LastEnqueue { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public int Workers { get; set; }
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

        public string Pid { get { return Id.Split('|').Length > 1 ? Id.Split('|')[1] : string.Empty; } }
        public string Id { get; set; }
        public string Service { get; set; }
        public string Controller { get { return Service.Split('/').Length > 1 ? Service.Split('/')[0] : string.Empty; } }
        public string Version { get { return Service.Split('/').Length > 1 ? Service.Split('/')[1] : string.Empty; } }
        public string Node { get; set; }
        public string Status { get; set; }
        public string Output { get; set; }
        public bool State
        {
            get
            {
                return string.Equals(Status, "passing", StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }

    public class ControllerInfo
    {
        public List<WorkerInfo> WorkerInfos { get; set; }
        public string Controller { get; set; }
        public string Name { get { return Controller.Split('/').Length > 1 ? Controller.Split('/')[0] : Controller; } }
        public int Count { get { return WorkerInfos.Count; } }
        public int Errors
        {
            get
            {
                return WorkerInfos.Count(c =>
                    !string.Equals(c.Status, "passing", StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}