namespace Gateway.Web.Models.Batches
{
    public class BatchDeleteConfigModel
    {
        public long ConfigurationId { get; set; }
        public string Type { get; set; }
        public int ScheduleCount { get; set; }
    }
}