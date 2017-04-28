namespace Gateway.Web.Models.Controller
{
    public class Version
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Status { get; set; }
        public System.Version SemVar { get; set; }

        public bool IsActive
        {
            get { return Status == "Active"; }
        }

        public Version(long id, string name, string alias, string status)
        {
            Id = id;
            Name = name;
            Alias = alias;
            Status = status;

            System.Version v;
            if (System.Version.TryParse(name, out v))
                SemVar = v;
            else
                SemVar = new System.Version();
        }
    }
}