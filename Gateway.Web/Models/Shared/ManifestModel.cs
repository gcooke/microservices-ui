namespace Gateway.Web.Models.Shared
{
    public abstract class ManifestModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public abstract string Type { get; }
    }
}