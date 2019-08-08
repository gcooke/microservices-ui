using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Gateway.Web.Models.Auth
{
    public class JwtIdentity : IIdentity
    {

        private string _name;
        private string _authenticationType;
        private bool _isAuthenticated;

        public JwtIdentity()
        {

        }

        public JwtIdentity(bool isAuthenticated, string name, string authenticationType)
        {
            _isAuthenticated = isAuthenticated;
            _name = name;
            _authenticationType = authenticationType;
        }


        public string Name => _name;

        public string AuthenticationType => _authenticationType;

        public bool IsAuthenticated => _isAuthenticated;
    }
}