using Gateway.Web.Enums;

namespace Gateway.Web.Utils
{
    public static class MonitoringHtmlEx
    {
        public static string GetStatusColor(this MonitoringStatus status)
        {
            if (status == MonitoringStatus.Ok)
            {
                return "darkgreen";
            }

            if (status == MonitoringStatus.Issue)
            {
                return "red";
            }

            if (status == MonitoringStatus.Warning)
            {
                return "orange";
            }

            return string.Empty;
        }
    }
}