using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Gateway.Web.Database
{
    public partial class GatewayEntities : DbContext
    {
        public GatewayEntities(string connectionString)
            : base(connectionString)
        {
            this.Database.CommandTimeout = 60;
        }
    }
}