using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.DataFeeds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Services
{
    public class DataFeedService : IDataFeedService
    {
        private readonly IGateway _gateway;
        private readonly ILogger _logger;
        private readonly string ControllerName = "DataFeedStatistics";

        public DataFeedService(IGateway gateway, ILoggingService loggingService)
        {
            _gateway = gateway;
            _logger = loggingService.GetLogger(this);
            //For testing
            //_gateway.SetGatewayUrlForService(ControllerName, "http://localhost:7000/");
        }

        public DataFeedHeader FetchDataFeedByHeaderId(long headerId)
        {
            var query = $@"DataFeedStatistics/FetchDataFeedStatisticsByHeaderId/{headerId}";
            var cube = FetchCube(query);

            return GenerateDataFeedHeader(cube);
        }

        public DataFeedHeader FetchDataFeedByRunDateAndType(DateTime runDate, int dataFeedType)
        {
            var query = $@"DataFeedStatistics/FetchDataFeedStatisticsByDataFeedType/{runDate.ToString("yyyy-MM-dd")}/{dataFeedType}";
            var cube = FetchCube(query);

            return GenerateDataFeedHeader(cube);
        }

        private ICube FetchCube(string query)
        {
            ICube cube = null;

            var resultTask = _gateway.Get<ICube>(ControllerName, query).GetAwaiter().GetResult();
            if (resultTask.Successfull)
                cube = resultTask.Body;
            else
                throw new Exception(resultTask.Message);

            return cube;
        }

        private List<DataFeedSummary> GenerateDataFeedSummary(ICube cube)
        {
            var list = new List<DataFeedSummary>();
            foreach (var row in cube.GetRows())
            {
                long headerId = 0;
                long.TryParse(row["HeaderId"]?.ToString(), out headerId);
                var name = row["DataFeedType"]?.ToString();
                if (string.IsNullOrEmpty(name) || name == "0")
                    name = row["DataFeedName"]?.ToString();
                list.Add(new DataFeedSummary(name)
                {
                    HeaderId = headerId,
                    Message = row["Message"]?.ToString(),
                    Duration = row["Duration"]?.ToString(),
                    Name = name,
                    Status = row["Status"]?.ToString(),
                    DataFeedName = row["DataFeedName"]?.ToString()
                });
            }
            return list;
        }

        private DataFeedHeader GenerateDataFeedHeader(ICube cube)
        {
            var headerCube = cube.GetRows().FirstOrDefault();
            int.TryParse(headerCube["DataFeedTypeId"]?.ToString(), out var dataFeedTypeId);

            var header = new DataFeedHeader()
            {
                DataFeedName = headerCube["DataFeedName"]?.ToString(),
                DataFeedStatus = headerCube["DataFeedStatus"]?.ToString(),
                DataFeedType = headerCube["DataFeedType"]?.ToString(),
                DataFeedTypeId = dataFeedTypeId,
                Message = headerCube["DataFeedMessage"]?.ToString(),
                DataFeedDetail = new List<DataFeedDetail>()
            };

            if (DateTime.TryParse(headerCube["DataFeedStarted"]?.ToString(), out var datafeedStarted))
                header.Started = datafeedStarted;
            if (DateTime.TryParse(headerCube["DataFeedEnded"]?.ToString(), out var datafeedEnded))
                header.Ended = datafeedStarted;

            foreach (var row in cube.GetRows())
            {
                int.TryParse(row["DestinationTotalRows"]?.ToString(), out var destinationTotalRows);
                int.TryParse(row["SourceTotalRows"]?.ToString(), out var sourceTotalRows);
                decimal.TryParse(row["TimeTakenInSeconds"]?.ToString(), out var timeTakenInSeconds);
                bool.TryParse(row["IsSuccessful"]?.ToString(), out var isSuccessful);
                var detail = new DataFeedDetail()
                {
                    Message = row["Message"]?.ToString(),
                    SourceName = row["SourceName"]?.ToString(),
                    SourceTotalRows = sourceTotalRows,
                    DestinationTotalRows = destinationTotalRows,
                    DestinationName = row["DestinationName"]?.ToString(),
                    TimeTakenInSeconds = timeTakenInSeconds,
                    IsSuccessful = isSuccessful
                };

                if (DateTime.TryParse(headerCube["Started"]?.ToString(), out var started))
                    detail.Started = started;
                if (DateTime.TryParse(headerCube["Ended"]?.ToString(), out var ended))
                    detail.Ended = ended;

                header.DataFeedDetail.Add(detail);
            }

            return header;
        }

        public List<DataFeedSummary> FetchDataFeedSummary(DateTime runDate)
        {
            var query = $@"DataFeedStatistics/FetchDataFeedStatisticsSummary/{runDate.ToString("yyyy-MM-dd")}";
            var cube = FetchCube(query);

            return GenerateDataFeedSummary(cube);
        }
    }
}