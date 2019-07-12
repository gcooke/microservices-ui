namespace Gateway.Web.Models.Export
{
    public class ExportResponse
    {
        public string Message { get; set; }
        public bool Successful { get; set; }
        private ExportSchedule ExportSchedule { get; set; }
    }
}