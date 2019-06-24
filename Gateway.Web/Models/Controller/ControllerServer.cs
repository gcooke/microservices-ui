namespace Gateway.Web.Models.Controller
{
    public class ControllerServer
    {
        public bool Allowed { get; set; }
        public int CPUCores { get; set; }
        public string Domain { get; set; }
        public int RAM { get; set; }
        public int ServerId { get; set; }
        public string ServerName { get; set; }
    }
}