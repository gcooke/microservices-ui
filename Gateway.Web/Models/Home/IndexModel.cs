using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Models.Home
{
    public class IndexModel
    {
        public IndexModel()
        {
            Controllers = new List<string>();
        }

        public IEnumerable<DashboardModel.NavigationCharacter> Characters
        {
            get
            {
                var list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var contains = Controllers.Select(c => c[0]).Distinct().OrderBy(n => n);
                foreach (var character in list)
                {
                    yield return new DashboardModel.NavigationCharacter(character, contains.Contains(character));
                }
            }
        }

        public List<string> Controllers { get; private set; }
    }
}