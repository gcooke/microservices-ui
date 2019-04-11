using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Models
{
    public class Issue
    {
        public string Description { get; set; }
        public bool ShouldContinueCheckingIssues { get; set; }
        public MonitoringLevel MonitoringLevel { get; set; }
        public string Remediation { get; set; }

        public Issue()
        {
            ShouldContinueCheckingIssues = true;
        }

        public bool HasRemediation => !string.IsNullOrWhiteSpace(Remediation);
    }
}