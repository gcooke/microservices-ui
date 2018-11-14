using System;
using System.Collections.Generic;

namespace Gateway.Web.Database
{
    public class BatchIssues
    {
        public BatchCommentDto Issue { get; set; }
        public IList<BatchCommentDto> Comments { get; set; }
        public Dictionary<Guid, string> Occurences { get; set; }
        public int OccurenceCount => Occurences.Count;

        public BatchIssues()
        {
            Comments = new List<BatchCommentDto>();
            Occurences = new Dictionary<Guid, string>();
        }
    }
}