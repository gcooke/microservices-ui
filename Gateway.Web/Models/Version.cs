namespace Gateway.Web.Models
{
    public class Version
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Status { get; set; }

        public Version(long id, string name, string alias, string status)
        {
            Id = id;
            Name = name;
            Alias = alias;
            Status = status;
        }
    }
}