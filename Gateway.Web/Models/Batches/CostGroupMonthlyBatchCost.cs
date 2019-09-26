namespace Gateway.Web.Models.Batches
{
    public class CostGroupMonthlyBatchCost
    {
        public string CostGroup { get; set; }
        public string BatchType { get; set; }
        public string CostType { get; set; }
        public decimal January { get; set; }
        public decimal February { get; set; }
        public decimal March { get; set; }
        public decimal April { get; set; }
        public decimal May { get; set; }
        public decimal June { get; set; }
        public decimal July { get; set; }
        public decimal August { get; set; }
        public decimal September { get; set; }
        public decimal October { get; set; }
        public decimal November { get; set; }
        public decimal December { get; set; }
        public decimal EstimatedAnnualTotal { get; set; }
    }
}