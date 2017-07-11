namespace Gateway.Web.Services
{
    public class VersionUpdate
    {
        public VersionUpdate(string controller, string version, string status, string alias)
        {
            Controller = controller;
            Version = version;
            Status = status;
            Alias = alias;
        }

        public string Controller { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public string Alias { get; set; }
    }    
}