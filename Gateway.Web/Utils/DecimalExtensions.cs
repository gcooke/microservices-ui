namespace Gateway.Web.Utils
{
    public static class DecimalExtensions
    {
        public static string StringValue(this decimal d) => d > 0 ? d.ToString("N2") : "";
    }
}