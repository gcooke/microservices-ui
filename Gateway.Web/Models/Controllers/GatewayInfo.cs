
/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models", IsNullable = false)]
public partial class ArrayOfGatewayInfo
{
    private GatewayInfo[] gatewayInfoField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("GatewayInfo")]
    public GatewayInfo[] GatewayInfo
    {
        get
        {
            return this.gatewayInfoField;
        }
        set
        {
            this.gatewayInfoField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
public partial class GatewayInfo
{
    private ControllerProxyState[] controllerProxyStatesField;

    private GatewayNode gatewayNodeField;

    private GatewayNodeService[] gatewayNodeServicesField;

    private PerformanceCounters performanceCountersField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    [System.Xml.Serialization.XmlArrayItemAttribute("ControllerProxyState", IsNullable = false)]
    public ControllerProxyState[] ControllerProxyStates
    {
        get
        {
            return this.controllerProxyStatesField;
        }
        set
        {
            this.controllerProxyStatesField = value;
        }
    }

    /// <remarks/>
    public GatewayNode GatewayNode
    {
        get
        {
            return this.gatewayNodeField;
        }
        set
        {
            this.gatewayNodeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    [System.Xml.Serialization.XmlArrayItemAttribute("GatewayNodeService", IsNullable = false)]
    public GatewayNodeService[] GatewayNodeServices
    {
        get
        {
            return this.gatewayNodeServicesField;
        }
        set
        {
            this.gatewayNodeServicesField = value;
        }
    }


    /// <remarks/>
    public PerformanceCounters PerformanceCounters
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

}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
public partial class ControllerProxyState
{

    private string controllerField;

    private ControllerProxyStateQueue _queueField;

    private string versionField;

    private WorkerState _field;

    /// <remarks/>
    public string Controller
    {
        get
        {
            return this.controllerField;
        }
        set
        {
            this.controllerField = value;
        }
    }

    /// <remarks/>
    public ControllerProxyStateQueue Queue
    {
        get
        {
            return this._queueField;
        }
        set
        {
            this._queueField = value;
        }
    }

    /// <remarks/>
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

    /// <remarks/>
    public WorkerState WorkerState
    {
        get
        {
            return this._field;
        }
        set
        {
            this._field = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
public partial class ControllerProxyStateQueue
{
    private System.DateTime lastDequeueField;

    private System.DateTime lastEnqueueField;

    private KeyValueOfRequestPriorityintfoOItFMr[] queueSizesField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers")]
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

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers")]
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

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers")]
    [System.Xml.Serialization.XmlArrayItemAttribute("KeyValueOfRequestPriorityintfoOItFMr", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays", IsNullable = false)]
    public KeyValueOfRequestPriorityintfoOItFMr[] QueueSizes
    {
        get
        {
            return this.queueSizesField;
        }
        set
        {
            this.queueSizesField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays", IsNullable = false)]
public partial class KeyValueOfRequestPriorityintfoOItFMr
{
    private string keyField;

    private byte valueField;

    /// <remarks/>
    public string Key
    {
        get
        {
            return this.keyField;
        }
        set
        {
            this.keyField = value;
        }
    }

    /// <remarks/>
    public byte Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
public partial class WorkerState
{
    private LiveWorkersProcessDetails[] liveWorkersField;

    private WorkerEventsWorkerStateEvent[] workerEventsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State")]
    [System.Xml.Serialization.XmlArrayItemAttribute("ProcessDetails", IsNullable = false)]
    public LiveWorkersProcessDetails[] LiveWorkers
    {
        get
        {
            return this.liveWorkersField;
        }
        set
        {
            this.liveWorkersField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State")]
    [System.Xml.Serialization.XmlArrayItemAttribute("WorkerStateEvent", IsNullable = false)]
    public WorkerEventsWorkerStateEvent[] WorkerEvents
    {
        get
        {
            return this.workerEventsField;
        }
        set
        {
            this.workerEventsField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State")]
public partial class LiveWorkersProcessDetails
{
    private string apiUrlField;

    private string controllerField;

    private uint pidField;

    private ushort portField;

    private object requestOriginField;

    private string serverField;

    private string versionField;

    private string workerIdField;

    /// <remarks/>
    public string ApiUrl
    {
        get
        {
            return this.apiUrlField;
        }
        set
        {
            this.apiUrlField = value;
        }
    }

    /// <remarks/>
    public string Controller
    {
        get
        {
            return this.controllerField;
        }
        set
        {
            this.controllerField = value;
        }
    }

    /// <remarks/>
    public uint Pid
    {
        get
        {
            return this.pidField;
        }
        set
        {
            this.pidField = value;
        }
    }

    /// <remarks/>
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

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object RequestOrigin
    {
        get
        {
            return this.requestOriginField;
        }
        set
        {
            this.requestOriginField = value;
        }
    }

    /// <remarks/>
    public string Server
    {
        get
        {
            return this.serverField;
        }
        set
        {
            this.serverField = value;
        }
    }

    /// <remarks/>
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

    /// <remarks/>
    public string WorkerId
    {
        get
        {
            return this.workerIdField;
        }
        set
        {
            this.workerIdField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State")]
public partial class WorkerEventsWorkerStateEvent
{
    private string eventField;

    private System.DateTime eventTimeField;

    private WorkerEventsWorkerStateEventWorker workerField;

    /// <remarks/>
    public string Event
    {
        get
        {
            return this.eventField;
        }
        set
        {
            this.eventField = value;
        }
    }

    /// <remarks/>
    public System.DateTime EventTime
    {
        get
        {
            return this.eventTimeField;
        }
        set
        {
            this.eventTimeField = value;
        }
    }

    /// <remarks/>
    public WorkerEventsWorkerStateEventWorker Worker
    {
        get
        {
            return this.workerField;
        }
        set
        {
            this.workerField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State")]
public partial class WorkerEventsWorkerStateEventWorker
{
    private string apiUrlField;

    private string controllerField;

    private uint pidField;

    private ushort portField;

    private object requestOriginField;

    private string serverField;

    private string versionField;

    private string workerIdField;

    /// <remarks/>
    public string ApiUrl
    {
        get
        {
            return this.apiUrlField;
        }
        set
        {
            this.apiUrlField = value;
        }
    }

    /// <remarks/>
    public string Controller
    {
        get
        {
            return this.controllerField;
        }
        set
        {
            this.controllerField = value;
        }
    }

    /// <remarks/>
    public uint Pid
    {
        get
        {
            return this.pidField;
        }
        set
        {
            this.pidField = value;
        }
    }

    /// <remarks/>
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

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object RequestOrigin
    {
        get
        {
            return this.requestOriginField;
        }
        set
        {
            this.requestOriginField = value;
        }
    }

    /// <remarks/>
    public string Server
    {
        get
        {
            return this.serverField;
        }
        set
        {
            this.serverField = value;
        }
    }

    /// <remarks/>
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

    /// <remarks/>
    public string WorkerId
    {
        get
        {
            return this.workerIdField;
        }
        set
        {
            this.workerIdField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
public partial class GatewayNode
{
    private string addressField;

    private string idField;

    private string nodeField;

    /// <remarks/>
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

    /// <remarks/>
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

    /// <remarks/>
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

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
public partial class GatewayNodeService
{
    private string checkIDField;

    private string nodeField;

    private string outputField;

    private string serviceIDField;

    private string serviceNameField;

    private string statusField;

    /// <remarks/>
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

    /// <remarks/>
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

    /// <remarks/>
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

    /// <remarks/>
    public string ServiceID
    {
        get
        {
            return this.serviceIDField;
        }
        set
        {
            this.serviceIDField = value;
        }
    }

    /// <remarks/>
    public string ServiceName
    {
        get
        {
            return this.serviceNameField;
        }
        set
        {
            this.serviceNameField = value;
        }
    }

    /// <remarks/>
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

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers", IsNullable = false)]
public partial class QueueSizes
{
    private KeyValueOfRequestPriorityintfoOItFMr[] keyValueOfRequestPriorityintfoOItFMrField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("KeyValueOfRequestPriorityintfoOItFMr", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
    public KeyValueOfRequestPriorityintfoOItFMr[] KeyValueOfRequestPriorityintfoOItFMr
    {
        get
        {
            return this.keyValueOfRequestPriorityintfoOItFMrField;
        }
        set
        {
            this.keyValueOfRequestPriorityintfoOItFMrField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State", IsNullable = false)]
public partial class LiveWorkers
{
    private LiveWorkersProcessDetails[] processDetailsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ProcessDetails")]
    public LiveWorkersProcessDetails[] ProcessDetails
    {
        get
        {
            return this.processDetailsField;
        }
        set
        {
            this.processDetailsField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Controllers.Workers.State", IsNullable = false)]
public partial class WorkerEvents
{
    private WorkerEventsWorkerStateEvent[] workerStateEventField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("WorkerStateEvent")]
    public WorkerEventsWorkerStateEvent[] WorkerStateEvent
    {
        get
        {
            return this.workerStateEventField;
        }
        set
        {
            this.workerStateEventField = value;
        }
    }
}

/// <remarks/>

[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/Gateway.Models", IsNullable = false)]

public partial class PerformanceCounters
{

    private decimal cpuUsageField;

    private ushort memUsageField;

    /// <remarks/>
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

    /// <remarks/>
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
