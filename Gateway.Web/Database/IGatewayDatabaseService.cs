using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Database
{
    public interface IGatewayDatabaseService
    {
        List<ControllerStats> GetControllerStatistics(DateTime start);

        List<HistoryItem> GetRecentRequests(string controller, DateTime start);

        ResponseStats GetResponseStats(DateTime start);

        RequestsChartModel GetControllerRequestSummary(string name, DateTime start);

        TimeChartModel GetControllerTimeSummary(string name, DateTime start);

        RequestModel GetRequestDetails(string name, string correlationId);

        Models.Controller.QueueChartModel GetControllerQueueSummary(string name, DateTime start);

        Models.Controllers.QueueChartModel GetControllerQueueSummary(DateTime start);
    }
}