namespace Gateway.Web.Services
{
    public class VersionUpdate
    {
        public VersionUpdate(string controller, string version, string status)
        {
            Controller = controller;
            Version = version;
            Status = status;
        }

        public string Controller { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
    }
}