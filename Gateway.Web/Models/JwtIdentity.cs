using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Gateway.Web.Models
{
    public class JwtIdentity : IIdentity
    {
        public JwtIdentity()
        {

        }

        public JwtIdentity(bool isAuthenticated, string name, string authenticationType)
        {
            IsAuthenticated = isAuthenticated;
            Name = name;
            AuthenticationType = authenticationType;
        }


        public string Name { get; }

        public string AuthenticationType { get; }

        public bool IsAuthenticated { get; }
    }
}