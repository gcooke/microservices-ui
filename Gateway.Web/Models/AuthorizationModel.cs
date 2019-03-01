using System.Collections.Generic;

namespace Gateway.Web.Models
{
    public class AuthorizationModel
    {
        public AuthorizationModel()
        {
            Roles = new List<string>();
        }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
    }
}