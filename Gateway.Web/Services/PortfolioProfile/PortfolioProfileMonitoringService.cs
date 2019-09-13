using System;
using System.Threading.Tasks;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Newtonsoft.Json;

namespace Gateway.Web.Services.PortfolioProfile
{
    public class PortfolioProfileMonitoringService : IPortfolioProfileMonitoringService
    {
        private readonly IGateway _gateway;

        public PortfolioProfileMonitoringService(IGateway gateway)
        {
            _gateway = gateway;
        }

        public PortfolioProfileReportStates GetStatus(string site, DateTime valuationDate, string portfolios)
        {
            var query = $"Monitoring/state/{site}/{valuationDate:yyyy-MM-dd}";
            var result = _gateway.Put<ICube, string>("portfolioprofile", query, portfolios).Result;

            if (!result.Successfull)
                return new PortfolioProfileReportStates(site, valuationDate);

            var states = new PortfolioProfileReportStates(site, valuationDate);
            foreach (var row in result.Body.GetRows())
            {
                var state = new PortfolioProfileReportState();
                state.LoadFromRow(row);
                states.States.Add(state);
            }

            return states;
        }

        public PortfolioProfileReport GetReport(string site, DateTime valuationDate, string portfolio, string report)
        {
            var query = $"Monitoring/report/{site}/{report}/{portfolio}/{valuationDate:yyyy-MM-dd}";
            var result = _gateway.Get<string>("portfolioprofile", query).Result;

            if (!result.Successfull)
                return new PortfolioProfileReport() {Data = $"Unable to obtain report: {result.Message}"};

            dynamic parsedJson = JsonConvert.DeserializeObject(result.Body);
            var formattedJson =  JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

            return new PortfolioProfileReport() { Data = formattedJson };
        }

        public Task<bool> Regenerate(string site, DateTime valuationDate, string portfolio, string report, string user)
        {
            return Task.Factory.StartNew(() =>
            {
                var query =
                    $"Profile/Generate/{site}/{valuationDate:yyyy-MM-dd}/{report}/{portfolio}?regeneratedBy={user}";
                var result = _gateway.Put<string, string>("portfolioprofile", query, string.Empty).Result;
                return result.Successfull;
            });
        }
    }
}