﻿using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Request;

namespace Gateway.Web.Database
{
    public interface IGatewayDatabaseService
    {
        List<ControllerStats> GetControllerStatistics(DateTime start);

        List<HistoryItem> GetRecentRequests(DateTime start);

        List<HistoryItem> GetRecentRequests(string controller, DateTime start);

        List<HistoryItem> GetRecentUserRequests(string user, DateTime start);

        ResponseStats GetResponseStats(DateTime start);

        AliasesModel GetAliases();

        RequestsChartModel GetControllerRequestSummary(string name, DateTime start);

        TimeChartModel GetControllerTimeSummary(string name, DateTime start);

        Summary GetRequestSummary(string correlationId);

        Children GetRequestChildren(string correlationId);

        Payloads GetRequestPayloads(string correlationId);

        Transitions GetRequestTransitions(string correlationId);

        PayloadData GetPayload(long id);

        Models.Controller.QueueChartModel GetControllerQueueSummary(string name, DateTime start);

        Models.Controllers.QueueChartModel GetControllerQueueSummary(DateTime start);

        IEnumerable<Status> GetVersionStatuses();

        bool HasStatusChanged(string controller, string version, string status, string alias);
    }
}