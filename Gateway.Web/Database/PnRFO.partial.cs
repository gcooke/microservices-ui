using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Gateway.Web.Database
{
    public partial class Entities : DbContext
    {
        public Entities(string connectionString)
            : base(connectionString)
        {
            this.Database.CommandTimeout = 60;
        }
    }
}