using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using Gateway.Web.Models.Group;

namespace Gateway.Web.Models.User
{
    [XmlType("User")]
    public class UserModel : IUserModel, IEquatable<UserModel>
    {
        public UserModel(long id)
        {
            Id = id;
        }

        public UserModel() { }

        public long Id { get; set; }

        public string Domain { get; set; }

        public string FullName { get; set; }

        public string Login { get; set; }

        public List<GroupModel> UserGroups { get; set; }

        public List<SelectListItem> Items { get; set; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(UserModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Domain, other.Domain) && string.Equals(Login, other.Login);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Domain != null ? Domain.GetHashCode() : 0) * 397) ^ (Login != null ? Login.GetHashCode() : 0);
            }
        }
    }
}