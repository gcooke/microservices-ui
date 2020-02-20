using Gateway.Web.Models.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Home
{
    public class IndexModel
    {
        public IndexModel()
        {
            Controllers = new List<ControllerDetail>();
        }

        public IEnumerable<DashboardModel.NavigationCharacter> Characters
        {
            get
            {
                var list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var contains = Controllers.Select(c => c.DisplayName[0]).Distinct().OrderBy(n => n);
                foreach (var character in list)
                {
                    yield return new DashboardModel.NavigationCharacter(character, contains.Contains(character));
                }
            }
        }

        public List<ControllerDetail> Controllers { get; private set; }
    }
}