using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        public IList<RequestConfiguration> GetRequestConfigurations()
        {
            using (var db = new GatewayEntities())
            {
                return db.RequestConfigurations.ToList();
            }
        }

        public RequestConfiguration GetRequestConfiguration(long id)
        {
            using (var db = new GatewayEntities())
            {
                var item = db.RequestConfigurations.SingleOrDefault(x => x.RequestConfigurationId == id);

                if (item == null)
                    throw new Exception($"Cannot find Request Configuration with ID {id}");

                return item;
            }
        }
    }
}