﻿namespace Gateway.Web.Models.User
{
    public class SitesModel : IUserModel
    {
        public SitesModel(long id)
        {
            Id = id;
        }

        public long Id { get; set; }
        public string Login { get { return Id.ToString(); } }
    }
}