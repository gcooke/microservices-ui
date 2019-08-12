namespace Gateway.Web.Controllers
{
    public class RequestedWorkers
    {
        public string ControllerName { get; internal set; }
        public string Version { get; internal set; }
        public int Instances { get; internal set; }

        public string Priority { get; internal set; }
    }
}