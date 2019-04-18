using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Pdc
{
    public class PdcServicesModel
    {
        public void Load(IEnumerable<PdcServiceModel> items)
        {
            Items = items.ToList();
        }

        public List<PdcServiceModel> Items { get; private set; }
    }

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