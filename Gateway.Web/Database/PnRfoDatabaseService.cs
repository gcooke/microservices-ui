using System;
using System.Collections.Generic;
using System.Linq;
using Bagl.Cib.MIT.IoC;

namespace Gateway.Web.Database
{
    public class PnRfoDatabaseService : IPnRFoDatabaseService
    {
        private readonly string ConnectionString = String.Empty;

        public PnRfoDatabaseService(ISystemInformation systemInformation)
        {
            ConnectionString = systemInformation.GetConnectionString("PnRFODatabase", "Database.PnRFO");
        }

        public List<string> GetControllerNames()
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                return database.Controllers.OrderBy(c => c.Name).Select(x => x.Name).ToList();
            }
        }
    }
}