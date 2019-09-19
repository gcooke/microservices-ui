namespace Gateway.Web.Models.Batches
{
    public class CostGroupBatchCost
    {
        public string CostGroup { get; set; }
        public string BatchType { get; set; }
        public string CostType { get; set; }
        public string Month { get; set; }
        public decimal TotalCost { get; set; }
    }
}