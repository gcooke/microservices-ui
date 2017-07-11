namespace Gateway.Web.Models.User
{
    public class GroupsModel : IUserModel
    {
        public GroupsModel(long id)
        {
            Id = id;
        }

        public long Id { get; set; }
        public string Login { get { return Id.ToString(); } }
    }
}