using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Batches
{
    public class BatchConfigList
    {
        public IList<BatchConfigModel> BatchConfigModels { get; set; } 

        public int TotalItems { get; set; }

        public int Offset { get; set; }

        public int PageSize { get; set; }

        public string SearchTerm { get; set; }

        public int PageCount => (int) Math.Ceiling(TotalItems / (double) PageSize);

        public int CurrentPage => (int)Math.Ceiling(Offset / (double)PageSize) + 1;

        public int NextPage => CurrentPage == PageCount ? CurrentPage : CurrentPage + 1;

        public int PreviousPage => CurrentPage == 1 ? CurrentPage : CurrentPage - 1;

    }
}