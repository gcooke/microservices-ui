namespace Gateway.Web.Models.User
{
    public class AddInsModel : IUserModel
    {
        public AddInsModel(long id)
        {
            Id = id;
        }

        public long Id { get; set; }
        public string Login { get { return Id.ToString(); } }
    }
}