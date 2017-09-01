using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controllers
{
    public class DashboardModel
    {
        public DashboardModel()
        {
            Controllers = new List<ControllerStats>();
            HistoryStartTime = DateTime.Today.AddDays(-7);
        }

        public IEnumerable<NavigationCharacter> Characters
        {
            get
            {
                var list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var contains = Controllers.Where(c => c.IsSeperator).Select(c => c.Name[0]).Distinct().OrderBy(n => n);
                foreach (var character in list)
                {
                    yield return new NavigationCharacter(character, contains.Contains(character));
                }
            }
        }

        public class NavigationCharacter
        {
            public char Character { get; private set; }
            public bool IsPresent { get; private set; }

            public NavigationCharacter(char character, bool isPresent)
            {
                Character = character;
                IsPresent = isPresent;
            }
        }

        public List<ControllerStats> Controllers { get; private set; }

        public DateTime HistoryStartTime { get; set; }
    }
}