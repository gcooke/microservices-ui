using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
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
        private readonly ILogger _logger;

        public PdcService(ISystemInformation information, ILoggingService loggingService)
        {
            _information = information;
            _loggingService = loggingService;
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
    }
}