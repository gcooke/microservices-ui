using System.Collections.Generic;
using Gateway.Web.Models.User;

namespace Gateway.Web.Models.Security
{
    public class UsersModel
    {
        public UsersModel()
        {
            Items = new List<UserModel>();
        }

        public List<UserModel> Items { get; private set; }
    }
}