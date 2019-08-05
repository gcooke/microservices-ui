namespace Gateway.Web.Models.Export
{
    public class FileExport
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string SourceInformation { get; set; }
        public string DestinationInformation { get; set; }
        public string Schedule { get; set; }
        public bool IsDisabled { get; set; }
        public string SuccessEmailAddress { get; set; }
        public string FailureEmailAddress { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime StartDateTime { get; set; }
    }
}