using System;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;

namespace Gateway.Web.Services.Batches.BatchCosts
{
    public class BatchCostsService : IBatchCostsService
    {
        private readonly IGateway _gateway;
        private readonly ISystemInformation _systemInformation;
        private readonly ILogger _logger;
        private readonly string _maintenanceControllerName = "Maintenance";

        public BatchCostsService(
                                IGateway gateway, 
                                ISystemInformation information, 
                                ILoggingService loggingService)
        {
            _gateway = gateway;
            _systemInformation = information;
            _logger = loggingService.GetLogger(this);

            _gateway.SetGatewayUrlForService(_maintenanceControllerName,"http://localhost:7000");
        }

        public ICube GetBatchCosts()
        {
            var query = "BatchReporting/GetBatchCosts";
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            _logger.Info($"Date passed as '{currentDate}'.");

            _logger.Info($"Starting request to {query}.");
            var response = _gateway.Post<ICube,string>(_maintenanceControllerName, query,currentDate).GetAwaiter().GetResult();
            _logger.Info($"{(response.Successfull?"(Successful)":"(Unsuccessful)")} Response received from '{query}'.");

            if (!response.Successfull)
                throw new Exception($"Error retrieving batch costs:{response.Message}");

            return response.Body;
        }

    }
}