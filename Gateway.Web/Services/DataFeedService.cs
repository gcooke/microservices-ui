using Gateway.Web.Models.DataFeeds;
using System;
using System.Collections.Generic;

namespace Gateway.Web.Services
{
    public class DataFeedService : IDataFeedService
    {
        public DataFeedHeader FetchDataFeedByHeaderId(long headerId)
        {
            var header = new DataFeedHeader()
            {
                DataFeedName = "TestMe",
                DataFeedStatus = "Successfull",
                DataFeedType = "SDS",
                Message = "All good",
                Started = DateTime.MaxValue,
                Ended = DateTime.MaxValue,
                TotalTimeTakenInSeconds = 5,
                DataFeedDetail = new List<DataFeedDetail>()
            };

            var detail = new List<DataFeedDetail>();
            detail.Add(new DataFeedDetail()
            {
                TimeTakenInSeconds = 20,
                Started = DateTime.MaxValue,
                Message = "Test Me please",
                Ended = DateTime.MaxValue,
                DestinationName = "Destinatio 1 ",
                DestinationTotalRows = 2,
                IsSuccessful = true,
                SourceName = "Source 1",
                SourceTotalRows = 76
            });

            detail.Add(new DataFeedDetail()
            {
                TimeTakenInSeconds = 45,
                Started = DateTime.MaxValue,
                Message = "Test Me please",
                Ended = DateTime.MaxValue,
                DestinationName = "Destinatio 3 ",
                DestinationTotalRows = 12,
                IsSuccessful = true,
                SourceName = "Source 3",
                SourceTotalRows = 4
            });

            detail.Add(new DataFeedDetail()
            {
                TimeTakenInSeconds = 7,
                Started = DateTime.MaxValue,
                Message = "Test Me please",
                Ended = DateTime.MaxValue,
                DestinationName = "Destinatio 2 ",
                DestinationTotalRows = 56,
                IsSuccessful = true,
                SourceName = "Source 2",
                SourceTotalRows = 12
            });

            header.DataFeedDetail.AddRange(detail);

            return header;
        }

        public DataFeedHeader FetchDataFeedByRunDateAndType(DateTime runDate, int dataFeedType)
        {
            var header = new DataFeedHeader()
            {
                DataFeedName = "TestMe",
                DataFeedStatus = "Successfull",
                DataFeedType = "SDS",
                Message = "All good",
                Started = DateTime.MaxValue,
                Ended = DateTime.MaxValue,
                TotalTimeTakenInSeconds = 5,
                DataFeedDetail = new List<DataFeedDetail>()
            };

            var detail = new List<DataFeedDetail>();
            detail.Add(new DataFeedDetail()
            {
                TimeTakenInSeconds = 20,
                Started = DateTime.MaxValue,
                Message = "Test Me please",
                Ended = DateTime.MaxValue,
                DestinationName = "Destinatio 1 ",
                DestinationTotalRows = 2,
                IsSuccessful = true,
                SourceName = "Source 1",
                SourceTotalRows = 76
            });

            detail.Add(new DataFeedDetail()
            {
                TimeTakenInSeconds = 45,
                Started = DateTime.MaxValue,
                Message = "Test Me please",
                Ended = DateTime.MaxValue,
                DestinationName = "Destinatio 3 ",
                DestinationTotalRows = 12,
                IsSuccessful = true,
                SourceName = "Source 3",
                SourceTotalRows = 4
            });

            detail.Add(new DataFeedDetail()
            {
                TimeTakenInSeconds = 7,
                Started = DateTime.MaxValue,
                Message = "Test Me please",
                Ended = DateTime.MaxValue,
                DestinationName = "Destinatio 2 ",
                DestinationTotalRows = 56,
                IsSuccessful = true,
                SourceName = "Source 2",
                SourceTotalRows = 12
            });

            header.DataFeedDetail.AddRange(detail);

            return header;
        }

        public List<DataFeedSummary> FetchDataFeedSummary(DateTime runDate)
        {
            var list = new List<DataFeedSummary>();

            list.Add(new DataFeedSummary("Pnrfo Sync")
            {
                Duration = "20 seconds",
                HistoryStartTime = DateTime.MaxValue,
                Message = "Test Me please",
                Name = "Pnrfo Sync",
                Status = "Successful"
            });

            list.Add(new DataFeedSummary("SDS")
            {
                Duration = "60 seconds",
                HistoryStartTime = DateTime.MaxValue,
                Message = "Here we go",
                Name = "SDS",
                Status = "Successful"
            });

            return list;
        }
    }
}