using System.Collections.Generic;
using Gateway.Web.Models.User;

namespace Gateway.Web.Models.Group
{
    public class UsersModel : IGroupModel
    {
        public UsersModel(long id)
        {
            Id = id;
            Items = new List<UserModel>();
        }
        public long Id { get; set; }


        public List<UserModel> Items { get; private set; }
        public string Name { get; set; }
    }
}