using System.Data.Entity;

namespace Gateway.Web.Database
{
    public partial class GatewayEntities : DbContext
    {
        public GatewayEntities(string connectionString, int timeoutInSeconds = 60)
            : base(connectionString)
        {
            this.Database.CommandTimeout = timeoutInSeconds;
        }
    }
}