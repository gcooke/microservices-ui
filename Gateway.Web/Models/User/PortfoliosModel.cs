namespace Gateway.Web.Models.User
{
    public class PortfoliosModel : IUserModel
    {
        public PortfoliosModel(long id)
        {
            Id = id;
        }

        public long Id { get; set; }
        public string Login { get { return Id.ToString(); } }
    }
}