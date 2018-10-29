using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;

namespace Gateway.Web.Services.Schedule.Interfaces
{
    public interface IRequestConfigurationService
    {
        IList<RequestConfiguration> GetRequestConfigurations();
        RequestConfiguration GetRequestConfiguration(long id);
    }

    public class RequestConfigurationService : IRequestConfigurationService
    {
        public string ConnectionString;

        public RequestConfigurationService(ISystemInformation systemInformation)
        {
            ConnectionString = systemInformation.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public IList<RequestConfiguration> GetRequestConfigurations()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                return db.RequestConfigurations.ToList();
            }
        }

        public RequestConfiguration GetRequestConfiguration(long id)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var item = db.RequestConfigurations.SingleOrDefault(x => x.RequestConfigurationId == id);

                if (item == null)
                    throw new Exception($"Cannot find Request Configuration with ID {id}");

                return item;
            }
        }
    }
}