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
}