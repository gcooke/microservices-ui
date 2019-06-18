using Gateway.Web.Models.DataFeeds;
using System;
using System.Collections.Generic;

namespace Gateway.Web.Services
{
    public interface IDataFeedService
    {
        List<DataFeedSummary> FetchDataFeedSummary(DateTime runDate);

        DataFeedHeader FetchDataFeedByHeaderId(long headerId);

        DataFeedHeader FetchDataFeedByRunDateAndType(DateTime runDate, int dataFeedType);
    }
}