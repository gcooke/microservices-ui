﻿using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using QueueChartModel = Gateway.Web.Models.Controller.QueueChartModel;

namespace Gateway.Web.Database
{
    public interface IGatewayDatabaseService
    {
        List<ControllerStats> GetControllerStatistics(DateTime start);

        List<string> GetControllerNames();

        List<HistoryItem> GetRecentRequests(DateTime start);

        List<HistoryItem> GetRecentRequests(string controller, DateTime start);

        List<HistoryItem> GetRecentUserRequests(string user, DateTime start);

        ResponseStats GetResponseStats(DateTime start);

        AliasesModel GetAliases();

        RequestsChartModel GetControllerRequestSummary(string name, DateTime start);

        TimeChartModel GetControllerTimeSummary(string name, DateTime start);

        Summary GetRequestSummary(string correlationId);

        List<HistoryItem> GetRequestChildren(Guid correlationId);

        Payloads GetRequestPayloads(string correlationId);

        Transitions GetRequestTransitions(string correlationId);

        PayloadData GetPayload(long id);

        ReportsModel GetUsage();

        IDictionary<string, int> GetCurrentControllerQueueSize(DateTime endDateTime, IList<string> controllers);

        IDictionary<string, int> GetCurrentControllerQueueSize(DateTime endDateTime);

        QueueChartModel GetHistoricalControllerQueueSizes(DateTime endDateTime);

        QueueChartModel GetHistoricalControllerQueueSizes(DateTime endDateTime, IList<string> controllers);

        QueueChartModel GetHistoricalControllerVersionQueueSizes(DateTime endDateTime, string controller);

        QueueChartModel GetHistoricalControllerVersionQueueSizes(DateTime endDateTime, string controller, string[] versions);

        LiveQueueChartModel GetLiveControllerVersionQueueSizes(DateTime startDateTime, DateTime? endDateTime, string controllerName);

        LiveQueueChartModel GetLiveControllerVersionQueueSizes(DateTime startDateTime, DateTime? endDateTime, string controller, string[] versions);

        IEnumerable<Status> GetVersionStatuses();

        IEnumerable<string> GetVersions(string controllerName);

        IEnumerable<string> GetActiveVersions(string controllerName);

        IEnumerable<string> GetActiveVersions();

        bool HasStatusChanged(string controller, string version, string status, string alias);

    }
}