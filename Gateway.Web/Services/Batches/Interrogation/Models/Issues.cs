using System.Collections.Generic;

namespace Gateway.Web.Services.Batches.Interrogation.Models
{
    public class Issues
    {
        public IList<Issue> IssueList { get; set; }

        public Issues()
        {
            IssueList = new List<Issue>();
        }

        public void Add(Issue issue)
        {
            IssueList.Add(issue);
        }
    }
}