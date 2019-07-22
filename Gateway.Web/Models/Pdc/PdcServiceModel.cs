namespace Gateway.Web.Models.Pdc
{
    public class PdcServiceModel
    {
        public PdcServiceModel()
        {
            PingResult = PingResult.None;
        }
        public string HostName { get; set; }
        public int HostPort { get; set; }
        public PingResult PingResult { get; set; }
    }

    public enum PingResult
    {
        None = 1,
        Success = 2,
        Failure
    }
}