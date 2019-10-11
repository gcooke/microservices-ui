namespace Gateway.Web.Services
{
    public interface IUsernameService
    {
        string GetFullNameFast(string name);
        string GetFullName(string name);
    }
}