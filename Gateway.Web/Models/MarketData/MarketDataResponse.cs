
namespace Gateway.Web.Models.MarketData
{
    public class MarketDataResponse
    {
        public VerifiedMonikersResult VerifiedMonikersResult { get; set; }
        public bool Fallback { get; set; }
        public bool Successfull { get; set; }
        public string ErrorMessage { get; set; }
    }
}