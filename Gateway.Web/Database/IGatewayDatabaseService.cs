using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bagl.Cib.MSF.ClientAPI.Model;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Home;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.ServerResource;
using QueueChartModel = Gateway.Web.Models.Controller.QueueChartModel;

namespace Gateway.Web.Database
{
    public interface IGatewayDatabaseService
    {
        IList<spGetHistoryTimingForSchedule_Result> GetHistoryTimingForSchedule(long scheduleId, int days);
        List<ControllerStats> GetControllerStatistics(DateTime start);

        List<string> GetControllerNames();

        List<HistoryItem> GetRecentRequests(DateTime start);

        List<HistoryItem> GetRecentRequests(string controller, DateTime start, string search = null);

        List<HistoryItem> GetRecentUserRequests(string user, DateTime start);

        ResponseStats GetResponseStats(DateTime start);

        AliasesModel GetAliases();

        RequestsChartModel GetControllerRequestSummary(string name, DateTime start);

        TimeChartModel GetControllerTimeSummary(string name, DateTime start);

        LinksModel GetLinks();

        void DeleteLink(long id);

        void AddLink(LinkModel link);

        GatewayRequest GetRequestClone(string correlationId);

        Summary GetRequestSummary(string correlationId);

        List<HistoryItem> GetRequestChildren(Guid correlationId);

        Payloads GetRequestPayloads(string correlationId);

        Transitions GetRequestTransitions(string correlationId);

        PayloadData GetPayload(long id);

        string GetBatchName(long scheduleId);

        ReportsModel GetUsage();

        QueueChartModel GetQueueChartModel(DateTime startDateUtc);

        QueueChartModel GetQueueChartModel(DateTime startDateUtc, IList<string> controllers);

        //QueueChartModel GetQueueChartModel(DateTime endDateTime, string controller);

        //QueueChartModel GetQueueChartModel(DateTime endDateTime, string controller, string[] versions);

        //LiveQueueChartModel GetLiveControllerVersionQueueSizes(DateTime startDateTime, DateTime? endDateTime, string controllerName);

        //LiveQueueChartModel GetLiveControllerVersionQueueSizes(DateTime startDateTime, DateTime? endDateTime, string controller, string[] versions);

        IEnumerable<Status> GetVersionStatuses();

        IEnumerable<ControllerState> GetControllerStates(IDictionary<string, ServerDiagnostics> serverDiagnostics);

        IEnumerable<string> GetVersions(string controllerName);

        IEnumerable<string> GetActiveVersions(string controllerName);

        IEnumerable<string> GetActiveVersions();

        bool HasStatusChanged(string controller, string version, string status, string alias);

        Task<List<ExtendedBatchSummary>> GetBatchSummaryStatsAsync(DateTime valuationDate);
       
        ControllerServersModel GetControllerServers(string controllerName);

        void UpdateControllerServers(ControllerServersModel controllerServers);

        IList<Server> GetServers();

        ServerControllerModel GetSeverControllers(int serverId);

        void UpdateServerControllers(ServerControllerModel serverControllerModel);

         IEnumerable<T> GetChildMessages<T>(Guid correlationId,
            Func<spGetRequestChildrenPayloadDetails_Result, T> converter);
    }
}
