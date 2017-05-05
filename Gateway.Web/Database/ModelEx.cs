using System.Linq;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Database
{
    public static class ModelEx
    {
        public static Models.Controllers.ControllerStats ToModel(this Controller controller, ResponseStats stats)
        {
            var result = new Models.Controllers.ControllerStats();

            result.Name = controller.Name;
            result.TotalCalls = stats.GetTotalCalls(controller.Name);
            result.TotalErrors = stats.GetTotalErrors(controller.Name);
            result.AverageResponse = stats.GetAverageResponse(controller.Name);
            foreach (var group in controller.Versions.GroupBy(v => v.Status.Name))
            {
                result.VersionSummary.Add(new InfoItem(group.Key, group.Count().ToString()));
            }
            return result;
        }


        public static Models.Controller.Version ToModel(this Version version)
        {
            var result = new Models.Controller.Version(version.Id,
                version.Version1,
                version.Alias,
                version.Status.Name);
            return result;
        }

        public static Models.Controller.HistoryItem ToModel(this spGetRecentRequests_Result item)
        {
            var result = new Models.Controller.HistoryItem();
            result.CorrelationId = item.CorrelationId;
            result.User = item.User;
            result.IpAddress = item.IpAddress;
            result.Controller = item.Controller;
            result.Version = item.Version;
            result.Resource = item.Resource;
            result.RequestType = item.RequestType;
            result.Priority = item.Priority;
            result.IsAsync = item.IsAsync;
            result.StartUtc = item.StartUtc;
            result.EndUtc = item.EndUtc;
            result.QueueTimeMs = item.QueueTimeMs;
            result.TimeTakeMs = item.TimeTakeMs;
            result.ResultCode = item.ResultCode;
            result.ResultMessage = item.ResultMessage;
            return result;
        }

        public static Models.Controller.HistoryItem ToModel(this spGetRecentRequestsAll_Result item)
        {
            var result = new Models.Controller.HistoryItem();
            result.CorrelationId = item.CorrelationId;
            result.User = item.User;
            result.IpAddress = item.IpAddress;
            result.Controller = item.Controller;
            result.Version = item.Version;
            result.Resource = item.Resource;
            result.RequestType = item.RequestType;
            result.Priority = item.Priority;
            result.IsAsync = item.IsAsync;
            result.StartUtc = item.StartUtc;
            result.EndUtc = item.EndUtc;
            result.QueueTimeMs = item.QueueTimeMs;
            result.TimeTakeMs = item.TimeTakeMs;
            result.ResultCode = item.ResultCode;
            result.ResultMessage = item.ResultMessage;
            return result;
        }

        public static Models.Controller.HistoryItem ToModel(this spGetRecentUserRequests_Result item)
        {
            var result = new Models.Controller.HistoryItem();
            result.CorrelationId = item.CorrelationId;
            result.User = item.User;
            result.IpAddress = item.IpAddress;
            result.Controller = item.Controller;
            result.Version = item.Version;
            result.Resource = item.Resource;
            result.RequestType = item.RequestType;
            result.Priority = item.Priority;
            result.IsAsync = item.IsAsync;
            result.StartUtc = item.StartUtc;
            result.EndUtc = item.EndUtc;
            result.QueueTimeMs = item.QueueTimeMs;
            result.TimeTakeMs = item.TimeTakeMs;
            result.ResultCode = item.ResultCode;
            result.ResultMessage = item.ResultMessage;
            return result;
        }
    }

}
