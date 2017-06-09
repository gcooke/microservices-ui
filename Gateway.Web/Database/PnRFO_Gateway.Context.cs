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
        public GatewayEntities()
            : base("name=GatewayEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Controller> Controllers { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<Response> Responses { get; set; }
        public virtual DbSet<StatusChange> StatusChanges { get; set; }
        public virtual DbSet<Version> Versions { get; set; }
        public virtual DbSet<QueueSize> QueueSizes { get; set; }
        public virtual DbSet<Payload> Payloads { get; set; }
        public virtual DbSet<Status> Status { get; set; }
    
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
    
        public virtual ObjectResult<spGetRequestStatsAll_Result> spGetRequestStatsAll(Nullable<System.DateTime> start)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRequestStatsAll_Result>("spGetRequestStatsAll", startParameter);
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
    
        public virtual ObjectResult<spGetRecentRequests_Result> spGetRecentRequests(Nullable<System.DateTime> start, string controller)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(System.DateTime));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetRecentRequests_Result>("spGetRecentRequests", startParameter, controllerParameter);
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
    }
}
