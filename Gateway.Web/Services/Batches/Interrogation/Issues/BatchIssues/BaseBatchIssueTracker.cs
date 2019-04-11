using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;
﻿using System.Collections.Generic;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Models;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    public abstract class BaseBatchIssueTracker : IIssueTracker<Batch>
    {
        private BatchInterrogationContext _context;

        public abstract Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item, BatchRun run);

        public abstract IEnumerable<string> GetDescriptions();
        public abstract int GetSequence();

        public virtual void SetContext(BatchInterrogationContext context)
        {
            _context = context ?? throw new Exception("Context cannot be null");
        }

        protected BatchInterrogationContext Context => _context;
    }

    public class RequestResponse
    {
        public Request Request { get; set; }
        public Response Response { get; set; }

        public RequestResponse(Request request, Response response)
        {
            Request = request;
            Response = response;
        }
    }

    public class BatchInterrogationContext
    {
        private readonly Guid? _correlationId;
        private readonly GatewayEntities _gatewayDb;
        private readonly Entities _pnrFoDb;

        public Lazy<Request> TradeStoreRequest { get; }
        public Lazy<RequestResponse> TradeStoreResponse { get; }
        public Lazy<IList<Request>> PricingRequests { get; }
        public Lazy<IList<RequestResponse>> PricingResponses { get; }
        public Lazy<IList<Request>> RiskDataRequests { get; }
        public Lazy<IList<RequestResponse>> RiskDataResponses { get; }

        
        public BatchInterrogationContext(Guid? correlationId, GatewayEntities gatewayDb, Entities pnrFoDb)
        {
            _correlationId = correlationId;
            _gatewayDb = gatewayDb;
            _pnrFoDb = pnrFoDb;

            TradeStoreRequest = new Lazy<Request>(GetTradeStoreRequest);
            TradeStoreResponse = new Lazy<RequestResponse>(GetTradeStoreResponse);
            PricingRequests = new Lazy<IList<Request>>(GetPricingRequest);
            PricingResponses = new Lazy<IList<RequestResponse>>(GetPricingResponses);
            RiskDataRequests = new Lazy<IList<Request>>(GetRiskDataRequests);
            RiskDataResponses = new Lazy<IList<RequestResponse>>(GetRiskDataResponses);
        }

        private RequestResponse GetTradeStoreResponse()
        {
            if (_correlationId == null) return null;

            var tradeStoreRequest = GetTradeStoreRequest();

            if (tradeStoreRequest == null) return null;

            var response = _gatewayDb
                .Responses
                .FirstOrDefault(x => x.CorrelationId == tradeStoreRequest.CorrelationId);

            return new RequestResponse(tradeStoreRequest, response);
        }

        private IList<RequestResponse> GetRiskDataResponses()
        {
            if (_correlationId == null) return new List<RequestResponse>();

            var requests = GetRiskDataRequests();
            var requestIdList = requests.Select(x => x.CorrelationId);
            var responses = _gatewayDb
                .Responses
                .Where(x => requestIdList.Contains(x.CorrelationId))
                .ToList();

            var requestResponses = new List<RequestResponse>();
            foreach (var response in responses)
            {
                var request = requests.First(x => x.CorrelationId == response.CorrelationId);
                requestResponses.Add(new RequestResponse(request, response));
            }

            return requestResponses;
        }

        private IList<RequestResponse> GetPricingResponses()
        {
            if (_correlationId == null) return new List<RequestResponse>();

            var requests = GetPricingRequest();
            var requestIdList = requests.Select(x => x.CorrelationId);
            var responses = _gatewayDb
                .Responses
                .Where(x => requestIdList.Contains(x.CorrelationId))
                .ToList();

            var requestResponses = new List<RequestResponse>();
            foreach (var response in responses)
            {
                var request = requests.First(x => x.CorrelationId == response.CorrelationId);
                requestResponses.Add(new RequestResponse(request, response));
            }

            return requestResponses;
        }

        private IList<Request> GetRiskDataRequests()
        {
            if (_correlationId == null) return new List<Request>();

            return _gatewayDb
                .Requests
                .Where(x => x.ParentCorrelationId == _correlationId)
                .Where(x => x.Controller.ToLower() == "riskdata")
                .Where(x => x.RequestType.ToLower() == "put")
                .Where(x => x.Resource.ToLower() != "BatchSummary/Save")
                .ToList();
        }

        private IList<Request> GetPricingRequest()
        {
            if (_correlationId == null) return new List<Request>();

            return _gatewayDb
                .Requests
                .Where(x => x.ParentCorrelationId == _correlationId)
                .Where(x => x.Controller.ToLower() == "pricing")
                .ToList();
        }

        private Request GetTradeStoreRequest()
        {
            if (_correlationId == null) return null;

            return _gatewayDb
                .Requests
                .Where(x => x.ParentCorrelationId == _correlationId)
                .FirstOrDefault(x => x.Controller.ToLower() == "tradestore");
        }
    }
}