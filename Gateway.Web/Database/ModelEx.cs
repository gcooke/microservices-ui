using System;
using System.Linq;
using System.Xml.Linq;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Home;
using Gateway.Web.Utils;

namespace Gateway.Web.Database
{
    public static class ModelEx
    {
        private static TimeSpan Hour = TimeSpan.FromHours(1);
        private static TimeSpan Minute = TimeSpan.FromMinutes(1);
        private static TimeSpan Second = TimeSpan.FromSeconds(1);

        public static Models.Controllers.ControllerStats ToModel(this Controller controller, ResponseStats stats)
        {
            var result = new Models.Controllers.ControllerStats();

            result.Name = controller.Name;
            result.TotalCalls = stats.GetTotalCalls(controller.Name);
            result.TotalErrors = stats.GetTotalErrors(controller.Name);
            result.AverageResponse = stats.GetAverageResponse(controller.Name);
            foreach (var group in controller.Versions.GroupBy(v => v.Status.Name))
            {
                if (group.Key == "Deleted") continue;
                result.VersionSummary.Add(new InfoItem(group.Key, group.Count().ToString()));
            }
            return result;
        }

        public static Models.Home.ControllerState ToModel(this spGetControllerStates_Result[] items)
        {
            if (items.Length == 1)
                return items[0].ToModel();

            string name = null;
            var firstFailure = DateTime.MaxValue;
            var now = DateTime.Now;
            foreach (var item in items.Where(i => i.Complete == 0))
            {
                name = item.Controller;
                if (item.MaxStart.HasValue && firstFailure > item.MaxStart)
                    firstFailure = item.MaxStart.Value;
            }

            var result = new Models.Home.ControllerState();
            result.Name = name;
            result.Link = "Controller/History/" + name;
            result.Time = firstFailure.ToString("dd MMM HH:mm");
            var diff = now - firstFailure;
            if (diff <= TimeSpan.Zero)
            {
                result.State = StateItemState.Okay;
                result.Text = "Okay";
            }
            else if (diff < TimeSpan.FromMinutes(1))
            {
                result.State = StateItemState.Warn;
                result.Text = diff.TimespanToString();
            }
            else
            {
                result.State = StateItemState.Error;
                result.Text = diff.TimespanToString();
            }
            return result;
        }

        public static Models.Home.ControllerState ToModel(this spGetControllerStates_Result item)
        {
            var result = new Models.Home.ControllerState();
            result.Name = item.Controller;
            result.Link = "Controller/History/" + result.Name;
            result.Time = item.MaxEnd?.ToString("dd MMM HH:mm");
            TimeSpan diff = TimeSpan.Zero;
            if (item.MaxStart.HasValue && item.MaxEnd.HasValue)
            {
                // Trim off milliseconds.
                var start = item.MaxStart.Value.AddMilliseconds(-item.MaxStart.Value.Millisecond);
                var end = item.MaxEnd.Value.AddMilliseconds(-item.MaxEnd.Value.Millisecond);
                diff = end - start;
            }
            if (diff >= TimeSpan.Zero)
            {
                result.State = StateItemState.Okay;
                result.Text = "Okay";
            }
            else if (diff > TimeSpan.FromSeconds(-5))
            {
                result.State = StateItemState.Warn;
                result.Text = diff.TimespanToString();
            }
            else
            {
                result.State = StateItemState.Error;
                result.Text = diff.TimespanToString();
            }
            return result;
        }

        private static string TimespanToString(this TimeSpan timespan)
        {
            var time = TimeSpan.FromTicks(Math.Abs(timespan.Ticks));
            if (time > Hour)
                return string.Format("{0:N2} hours", time.TotalHours);

            if (time > Minute)
                return string.Format("{0:N2} mins", time.TotalMinutes);

            if (time > Second)
                return string.Format("{0:N2} secs", time.TotalSeconds);

            return string.Format("{0:N0} ms", time.TotalMilliseconds);
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
            result.RequestChangeUtc = item.UpdateTimeUtc;
            result.RequestChangeMessage = item.Status;
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
            result.RequestChangeUtc = item.UpdateTimeUtc;
            result.RequestChangeMessage = item.Status;
            return result;
        }

        public static Models.Controller.HistoryItem ToModel(this spGetChildRequests_Result item)
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
            result.RequestChangeUtc = item.UpdateTimeUtc;
            result.RequestChangeMessage = item.Status;
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
            result.RequestChangeUtc = item.UpdateTimeUtc;
            result.RequestChangeMessage = item.Status;
            return result;
        }

        public static Models.Request.DetailRow ToModel(this spGetRequestChildSummary_Result item)
        {
            var result = new Models.Request.DetailRow();
            result.Controller = item.Controller;
            result.RequestCount = item.RequestCount;
            result.SuccessfulCount = item.SuccessfulCount;
            result.CompletedCount = item.Completed;
            result.SizeUnit = item.SizeUnit;
            result.Size = item.Size;
            if (item.MinStartUtc.HasValue)
                result.MinStart = item.MinStartUtc.Value.ToLocalTime();
            if (item.MaxEndUtc.HasValue)
                result.MaxEnd = item.MaxEndUtc.Value.ToLocalTime();

            return result;
        }

        public static Models.Request.RequestChangeModel ToModel(this RequestChange item)
        {
            var result = new Models.Request.RequestChangeModel();
            result.Id = item.Id;
            result.Time = item.UpdateTimeUtc.ToLocalTime();
            result.Message = item.Status;
            return result;
        }

        public static Models.Security.LinkModel ToModel(this Link item)
        {
            var result = new Models.Security.LinkModel();
            result.Id = item.Id;
            result.Name = item.Name;
            result.Type = item.Type;
            result.Glyph = item.Glyph;
            result.AdditionalData = item.AdditionalData;
            result.Permission = item.Permission;
            return result;
        }

        public static ExtendedBatchSummary ToModel(this BatchStat summary, Request request, Response response)
        {
            if (request == null)
                request = new Request();

            if (response == null)
                response = new Response { ResultMessage = "In progress" };

            var str = summary.Data.GetUncompressedString();
            var data = XElement.Parse(str);

            return new ExtendedBatchSummary
            {
                CorrelationId = summary.CorrelationId,
                StartUtc = summary.StartTime,
                EndUtc = summary.EndTime,
                Data = data,
                ControllerVersion = request.Version,
                Resource = request.Resource,
                Successfull = response.ResultCode == 1,
                Message = response.ResultMessage
            };
        }
    }

}
