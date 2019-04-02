﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gateway.Web.Database
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class GatewayEntities : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Controller> Controllers { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<Response> Responses { get; set; }
        public virtual DbSet<Version> Versions { get; set; }
        public virtual DbSet<Payload> Payloads { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<RequestChange> RequestChanges { get; set; }
        public virtual DbSet<StatusChange> StatusChanges { get; set; }
        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<BatchStat> BatchStats { get; set; }
        public virtual DbSet<ControllerExternalResource> ControllerExternalResources { get; set; }
        public virtual DbSet<ExternalResource> ExternalResources { get; set; }
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<ServerExternalResource> ServerExternalResources { get; set; }
        public virtual DbSet<ScheduleGroup> ScheduleGroups { get; set; }
        public virtual DbSet<RequestConfiguration> RequestConfigurations { get; set; }
        public virtual DbSet<ExecutableConfiguration> ExecutableConfigurations { get; set; }
        public virtual DbSet<RiskBatchConfiguration> RiskBatchConfigurations { get; set; }
        public virtual DbSet<RiskBatchSchedule> RiskBatchSchedules { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<ScheduledJob> ScheduledJobs { get; set; }
    
        public virtual ObjectResult<spGetJobStats_Result> spGetJobStats(Nullable<System.DateTime> start, string controller)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetJobStats_Result>("spGetJobStats", startParameter, controllerParameter);
        }
    
        public virtual ObjectResult<spGetJobStatsAll_Result> spGetJobStatsAll(Nullable<System.DateTime> start)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetJobStatsAll_Result>("spGetJobStatsAll", startParameter);
        }
    
        public virtual ObjectResult<spGetTimeStats_Result> spGetTimeStats(Nullable<System.DateTime> start, string controller)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetTimeStats_Result>("spGetTimeStats", startParameter, controllerParameter);
        }
    
        public virtual ObjectResult<spGetQueueCounts_Result> spGetQueueCounts(Nullable<System.DateTime> start, string controller)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetQueueCounts_Result>("spGetQueueCounts", startParameter, controllerParameter);
        }
    
        public virtual ObjectResult<spGetQueueCountsAll_Result> spGetQueueCountsAll(Nullable<System.DateTime> start)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetQueueCountsAll_Result>("spGetQueueCountsAll", startParameter);
        }
    
        public virtual ObjectResult<spGetResponseStatsAll_Result> spGetResponseStatsAll(Nullable<System.DateTime> start)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetResponseStatsAll_Result>("spGetResponseStatsAll", startParameter);
        }
    
        public virtual ObjectResult<spGetPayloads_Result> spGetPayloads(Nullable<System.Guid> correlationId)
        {
            var correlationIdParameter = correlationId.HasValue ?
                new ObjectParameter("CorrelationId", correlationId) :
                new ObjectParameter("CorrelationId", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetPayloads_Result>("spGetPayloads", correlationIdParameter);
        }
    
        public virtual ObjectResult<spGetRequestChildSummary_Result> spGetRequestChildSummary(Nullable<System.Guid> correllationId)
        {
            var correllationIdParameter = correllationId.HasValue ?
                new ObjectParameter("correllationId", correllationId) :
                new ObjectParameter("correllationId", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRequestChildSummary_Result>("spGetRequestChildSummary", correllationIdParameter);
        }
    
        public virtual ObjectResult<spGetChildRequests_Result> spGetChildRequests(Nullable<System.Guid> correlationId)
        {
            var correlationIdParameter = correlationId.HasValue ?
                new ObjectParameter("correlationId", correlationId) :
                new ObjectParameter("correlationId", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetChildRequests_Result>("spGetChildRequests", correlationIdParameter);
        }
    
        public virtual ObjectResult<spGetRecentRequestsAll_Result> spGetRecentRequestsAll(Nullable<System.DateTime> start, string controller)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRecentRequestsAll_Result>("spGetRecentRequestsAll", startParameter, controllerParameter);
        }
    
        public virtual ObjectResult<spGetRecentUserRequests_Result> spGetRecentUserRequests(Nullable<System.DateTime> start, string user)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var userParameter = user != null ?
                new ObjectParameter("User", user) :
                new ObjectParameter("User", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRecentUserRequests_Result>("spGetRecentUserRequests", startParameter, userParameter);
        }
    
        public virtual ObjectResult<spGetUserReport_Result> spGetUserReport(Nullable<System.DateTime> start)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetUserReport_Result>("spGetUserReport", startParameter);
        }
    
        public virtual ObjectResult<spGetControllerStates_Result> spGetControllerStates()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetControllerStates_Result>("spGetControllerStates");
        }
    
        public virtual ObjectResult<spGetJobCounts_Result> spGetJobCounts(Nullable<System.DateTime> start)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetJobCounts_Result>("spGetJobCounts", startParameter);
        }
    
        public virtual ObjectResult<spGetIncompleteRequestCount_Result> spGetIncompleteRequestCount(Nullable<System.DateTime> time)
        {
            var timeParameter = time.HasValue ?
                new ObjectParameter("time", time) :
                new ObjectParameter("time", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetIncompleteRequestCount_Result>("spGetIncompleteRequestCount", timeParameter);
        }
    
        public virtual ObjectResult<spGetRequestCounts_Result> spGetRequestCounts(Nullable<System.DateTime> start)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRequestCounts_Result>("spGetRequestCounts", startParameter);
        }
    
        public virtual ObjectResult<spGetRequestStats_Result> spGetRequestStats(Nullable<System.DateTime> start, string controller)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRequestStats_Result>("spGetRequestStats", startParameter, controllerParameter);
        }
    
        public virtual ObjectResult<spGetRecentRequests_Result> spGetRecentRequests(Nullable<System.DateTime> start, string controller, string searchString)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            var searchStringParameter = searchString != null ?
                new ObjectParameter("SearchString", searchString) :
                new ObjectParameter("SearchString", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRecentRequests_Result>("spGetRecentRequests", startParameter, controllerParameter, searchStringParameter);
        }
    }
}
