using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Pdc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Gateway.Web.Services.Pdc
{
    public class PdcService : IPdcService
    {
        private readonly ISystemInformation _information;
        private readonly ILoggingService _loggingService;
        private readonly IGateway _gateway;
        private readonly ILogger _logger;
        private readonly string _controller = "PDC";

        public PdcService(IGateway gateway, ISystemInformation information, ILoggingService loggingService)
        {
            _information = information;
            _loggingService = loggingService;
            _gateway = gateway;
            _logger = loggingService.GetLogger(this);
        }

        public IEnumerable<PdcServiceModel> PingAll(IEnumerable<PdcServiceModel> services)
        {
            var list = new List<Task>();
            foreach (var service in services)
            {
                try
                {
                    list.Add(Task.Factory.StartNew(() => Ping(service)));
                }
                catch (Exception)
                {
                    service.PingResult = PingResult.Failure;
                }
            }

            Task.WaitAll(list.ToArray());

            return services;
        }

        public void Ping(PdcServiceModel service)
        {
            var client = new PdcClient(_logger);
            try
            {
                var stream = client.Connect(_information.EnvironmentName, service.HostName, service.HostPort, SslProtocols.Tls);
                var message = client.Read(stream);
                stream.Close();
                client.Close();
                service.PingResult = PingResult.Success;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Cannot connect to {service.HostName}:{service.HostPort}");
                client.Close();
                service.PingResult = PingResult.Failure;
            }
        }

        public PdcServiceModel[] GetInstances()
        {
            var pdcInstances = _information.GetSetting("PdcServices");
            if (string.IsNullOrEmpty(pdcInstances))
                return new PdcServiceModel[0];
            return pdcInstances.Split(';').Select(endpoint =>
             {
                 var parts = endpoint.Split(':');
                 var port = parts.Length == 2 ? parts[1] : "9215";

                 return new PdcServiceModel
                 {
                     HostName = parts[0],
                     HostPort = int.Parse(port),
                     PingResult = PingResult.None
                 };
             }).ToArray();
        }

        public PdcTradesModel GetTradesSummary(DateTime asOf)
        {
            var query = $"trades/summary/asOf/{asOf:yyyy-MM-dd}";
            var response = _gateway.Get<ICube>(_controller, query).GetAwaiter().GetResult();
            var result = new PdcTradesModel
            {
                BusinessDate = asOf
            };
            if (!response.Successfull)
                throw new Exception($"Error extracting results:{response.Message}");

            var cube = response.Body;
            foreach (var row in cube.GetRows())
            {
                var trade = new PdcTradeModel
                {
                    BookingSystem = row.GetStringValue("BookingSystem"),
                    SiteName = row.GetStringValue("SiteName"),
                    SdsId = row.GetStringValue("SdsId"),
                    Counterparty = row.GetStringValue("Counterparty"),
                    RequestId = row.GetStringValue("RequestId"),
                    TradeId = row.GetStringValue("TradeId"),
                    VersionId = row.GetStringValue("VersionId"),
                    RequestDate = row.GetValue<DateTime>("RequestDate").GetValueOrDefault(),
                    Instrument = row.GetStringValue("Instrument"),
                    PredealCheckResult = row.GetValue<bool>("PredealCheckResult").GetValueOrDefault(),
                    PredealCheckReason = row.GetStringValue("PredealCheckReason"),
                    EntryDate = row.GetValue<DateTime>("EntryDate").GetValueOrDefault()
                };
                result.Items.Add(trade);
            }
            return result;
        }
    }
}